using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;

namespace SharedLibrary.Services;

/// <summary>
/// Implementation of Redis cache operations using StackExchange.Redis
/// </summary>
public class RedisCache : IRedisCache, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCache> _logger;
    private readonly RedisSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<string, List<ChannelMessageQueue>> _subscriptions;

    public RedisCache(
        IOptions<RedisSettings> settings,
        ILogger<RedisCache> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _subscriptions = new Dictionary<string, List<ChannelMessageQueue>>();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var options = new ConfigurationOptions
        {
            EndPoints = { _settings.ConnectionString },
            AbortOnConnectFail = _settings.AbortOnConnectFail,
            AllowAdmin = _settings.AllowAdmin,
            ConnectTimeout = _settings.ConnectTimeout,
            Ssl = _settings.UseSsl,
            DefaultDatabase = _settings.Database
        };

        _redis = ConnectionMultiplexer.Connect(options);
        _db = _redis.GetDatabase();

        _logger.LogInformation("Redis cache initialized with endpoint: {Endpoint}", _settings.ConnectionString);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var value = await _db.StringGetAsync(fullKey);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!, _jsonOptions) : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value for key: {Key}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, int? expirationMinutes = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var expiry = TimeSpan.FromMinutes(expirationMinutes ?? _settings.DefaultTtlMinutes);
            var keepTtl = expiry > TimeSpan.Zero;
            await _db.StringSetAsync(fullKey, serializedValue, expiry, keepTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _db.KeyDeleteAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _db.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of key: {Key}", key);
            throw;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? expirationMinutes = null)
    {
        var value = await GetAsync<T>(key);
        if (value != null) return value;

        value = await factory();
        await SetAsync(key, value, expirationMinutes);
        return value;
    }

    public async Task RemoveAllAsync(IEnumerable<string> keys)
    {
        try
        {
            var fullKeys = keys.Select(k => (RedisKey)GetFullKey(k)).ToArray();
            await _db.KeyDeleteAsync(fullKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing multiple keys");
            throw;
        }
    }

    public async Task<IDictionary<string, T?>> GetAllAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var fullKeys = keys.Select(k => (RedisKey)GetFullKey(k)).ToArray();
            var values = await _db.StringGetAsync(fullKeys);

            return keys.Zip(values, (k, v) => new
            {
                Key = k,
                Value = v.HasValue ? JsonSerializer.Deserialize<T>(v!, _jsonOptions) : default
            }).ToDictionary(x => x.Key, x => x.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple values");
            throw;
        }
    }

    public async Task SetAllAsync<T>(IDictionary<string, T> keyValues, int? expirationMinutes = null)
    {
        try
        {
            var expiry = TimeSpan.FromMinutes(expirationMinutes ?? _settings.DefaultTtlMinutes);
            var keepTtl = expiry > TimeSpan.Zero;
            var batch = _db.CreateBatch();

            foreach (var kv in keyValues)
            {
                var fullKey = GetFullKey(kv.Key);
                var serializedValue = JsonSerializer.Serialize(kv.Value, _jsonOptions);
                await batch.StringSetAsync(fullKey, serializedValue, expiry, keepTtl);
            }

            batch.Execute();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple values");
            throw;
        }
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        try
        {
            var fullChannel = GetFullKey(channel);
            var serializedMessage = JsonSerializer.Serialize(message, _jsonOptions);
            await _db.PublishAsync(RedisChannel.Literal(fullChannel), serializedMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to channel: {Channel}", channel);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string channel, Func<T, Task> handler)
    {
        try
        {
            var fullChannel = GetFullKey(channel);
            var sub = _redis.GetSubscriber();

            var queue = await sub.SubscribeAsync(RedisChannel.Literal(fullChannel));
            queue.OnMessage(async message =>
            {
                try
                {
                    var value = JsonSerializer.Deserialize<T>(message.Message!, _jsonOptions);
                    if (value != null)
                    {
                        await handler(value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message from channel: {Channel}", channel);
                }
            });

            if (!_subscriptions.ContainsKey(channel))
            {
                _subscriptions[channel] = new List<ChannelMessageQueue>();
            }
            _subscriptions[channel].Add(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to channel: {Channel}", channel);
            throw;
        }
    }

    public async Task UnsubscribeAsync(string channel)
    {
        try
        {
            if (_subscriptions.TryGetValue(channel, out var queues))
            {
                foreach (var queue in queues)
                {
                    await queue.UnsubscribeAsync();
                }
                _subscriptions.Remove(channel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from channel: {Channel}", channel);
            throw;
        }
    }

    public async Task<TimeSpan> GetTtlAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _db.KeyTimeToLiveAsync(fullKey) ?? TimeSpan.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            throw;
        }
    }

    public async Task SetExpirationAsync(string key, int minutes)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _db.KeyExpireAsync(fullKey, TimeSpan.FromMinutes(minutes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting expiration for key: {Key}", key);
            throw;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _db.StringIncrementAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing value for key: {Key}", key);
            throw;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _db.StringDecrementAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing value for key: {Key}", key);
            throw;
        }
    }

    public async Task SetAddAsync(string key, string value)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _db.SetAddAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding value to set for key: {Key}", key);
            throw;
        }
    }

    public async Task<IEnumerable<string>> SetMembersAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var values = await _db.SetMembersAsync(fullKey);
            return values.Select(v => (string)v!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting set members for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, int? expirationMinutes, bool onlyIfNotExists)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var expiry = TimeSpan.FromMinutes(expirationMinutes ?? _settings.DefaultTtlMinutes);

            return await _db.StringSetAsync(fullKey, serializedValue, expiry, onlyIfNotExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
            throw;
        }
    }

    private string GetFullKey(string key) => string.IsNullOrEmpty(_settings.InstanceName) ? key : $"{_settings.InstanceName}:{key}";

    public void Dispose()
    {
        foreach (var queues in _subscriptions.Values)
        {
            foreach (var queue in queues)
            {
                queue.Unsubscribe();
            }
        }
        _subscriptions.Clear();
        _redis.Dispose();
    }
}