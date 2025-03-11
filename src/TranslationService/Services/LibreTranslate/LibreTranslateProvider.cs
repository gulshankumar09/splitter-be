using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimit;
using TranslationService.Configuration;
using TranslationService.Interfaces;

namespace TranslationService.Services.LibreTranslate;

/// <summary>
/// Implementation of LibreTranslate API provider
/// </summary>
public class LibreTranslateProvider : ITranslationProvider
{
    private readonly HttpClient _httpClient;
    private readonly LibreTranslateSettings _settings;
    private readonly ILogger<LibreTranslateProvider> _logger;
    private readonly IMemoryCache _cache;
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncPolicy _rateLimitPolicy;
    private IList<string>? _supportedLanguages;

    private const string CacheKeyPrefix = "LibreTranslate_";
    private const string LanguagesCacheKey = CacheKeyPrefix + "Languages";
    private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromHours(24);

    public string ProviderName => "LibreTranslate";

    public LibreTranslateProvider(
        HttpClient httpClient,
        IOptions<LibreTranslateSettings> settings,
        ILogger<LibreTranslateProvider> logger,
        IMemoryCache cache)
    {
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;
        _httpClient = httpClient;

        // Configure base address and default headers
        _httpClient.BaseAddress = new Uri(_settings.ApiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"ApiKey {_settings.ApiKey}");
        }

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>(ex => IsTransientException(ex))
            .WaitAndRetryAsync(
                _settings.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(ex,
                        "Retry {RetryCount} after {Delay}s due to {Error}",
                        retryCount, timeSpan.TotalSeconds, ex.Message);
                });

        // Configure rate limiting policy
        _rateLimitPolicy = Policy.RateLimitAsync(
            numberOfExecutions: 60, // Adjust based on instance limits
            perTimeSpan: TimeSpan.FromMinutes(1));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        if (_supportedLanguages != null)
            return _supportedLanguages;

        return await _cache.GetOrCreateAsync(LanguagesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var languages = await ExecuteWithPolicies(() =>
                    _httpClient.GetFromJsonAsync<List<LanguageInfo>>("/languages"));

                _supportedLanguages = languages?
                    .Select(l => l.Code.ToLower())
                    .ToList() ?? new List<string>();

                return _supportedLanguages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported languages");
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        var cacheKey = GetTranslationCacheKey(text, sourceLanguage, targetLanguage);
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var request = new TranslationRequest
                {
                    Q = text,
                    Source = sourceLanguage == "auto" ? "auto" : sourceLanguage.ToLower(),
                    Target = targetLanguage.ToLower(),
                    Format = "text",
                    ApiKey = _settings.ApiKey
                };

                var response = await ExecuteWithPolicies(() =>
                    _httpClient.PostAsJsonAsync("/translate", request));

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<TranslationResponse>();
                return result?.TranslatedText ?? text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text. Source: {Source}, Target: {Target}",
                    sourceLanguage, targetLanguage);
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, string>> TranslateBatchAsync(
        IEnumerable<string> texts,
        string sourceLanguage,
        string targetLanguage)
    {
        try
        {
            var textList = texts.ToList();
            var results = new Dictionary<string, string>();

            // Process in batches according to settings
            for (int i = 0; i < textList.Count; i += _settings.MaxBatchSize)
            {
                var batch = textList.Skip(i).Take(_settings.MaxBatchSize).ToArray();
                var request = new BatchTranslationRequest
                {
                    Q = batch,
                    Source = sourceLanguage == "auto" ? "auto" : sourceLanguage.ToLower(),
                    Target = targetLanguage.ToLower(),
                    Format = "text",
                    ApiKey = _settings.ApiKey
                };

                var response = await ExecuteWithPolicies(() =>
                    _httpClient.PostAsJsonAsync("/translate", request));

                response.EnsureSuccessStatusCode();
                var batchResults = await response.Content
                    .ReadFromJsonAsync<List<TranslationResponse>>();

                if (batchResults != null)
                {
                    for (int j = 0; j < batch.Length; j++)
                    {
                        results[batch[j]] = batchResults[j].TranslatedText;
                    }
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch translating texts. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> DetectLanguageAsync(string text)
    {
        var cacheKey = $"{CacheKeyPrefix}Detect_{text.GetHashCode()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheTime;

            try
            {
                var request = new DetectionRequest
                {
                    Q = text,
                    ApiKey = _settings.ApiKey
                };

                var response = await ExecuteWithPolicies(() =>
                    _httpClient.PostAsJsonAsync("/detect", request));

                response.EnsureSuccessStatusCode();
                var detections = await response.Content
                    .ReadFromJsonAsync<List<DetectionResponse>>();

                return detections?.FirstOrDefault()?.Language.ToLower() ?? "und";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting language");
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        var languages = await GetSupportedLanguagesAsync();
        return languages.Contains(languageCode.ToLower());
    }

    private async Task<T> ExecuteWithPolicies<T>(Func<Task<T>> action)
    {
        return await _rateLimitPolicy
            .WrapAsync(_retryPolicy)
            .ExecuteAsync(action);
    }

    private static string GetTranslationCacheKey(string text, string sourceLanguage, string targetLanguage)
    {
        return $"{CacheKeyPrefix}Trans_{text.GetHashCode()}_{sourceLanguage}_{targetLanguage}";
    }

    private static bool IsTransientException(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => true,
            TimeoutException => true,
            TaskCanceledException => true,
            _ => false
        };
    }

    #region API Models

    private class LanguageInfo
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    private class TranslationRequest
    {
        [JsonPropertyName("q")]
        public string Q { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";

        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }
    }

    private class BatchTranslationRequest
    {
        [JsonPropertyName("q")]
        public string[] Q { get; set; } = Array.Empty<string>();

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";

        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }
    }

    private class TranslationResponse
    {
        [JsonPropertyName("translatedText")]
        public string TranslatedText { get; set; } = string.Empty;

        [JsonPropertyName("detectedLanguage")]
        public DetectionResponse? DetectedLanguage { get; set; }
    }

    private class DetectionRequest
    {
        [JsonPropertyName("q")]
        public string Q { get; set; } = string.Empty;

        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }
    }

    private class DetectionResponse
    {
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    #endregion
}