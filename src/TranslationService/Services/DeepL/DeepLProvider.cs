using DeepL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using TranslationService.Configuration;
using TranslationService.Interfaces;

namespace TranslationService.Services.DeepL;

/// <summary>
/// Implementation of DeepL API provider
/// </summary>
public class DeepLProvider : ITranslationProvider, IDisposable
{
    private readonly ITranslator _client;
    private readonly DeepLSettings _settings;
    private readonly ILogger<DeepLProvider> _logger;
    private readonly IMemoryCache _cache;
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncPolicy _rateLimitPolicy;
    private IList<string>? _supportedLanguages;

    private const string CacheKeyPrefix = "DeepL_";
    private const string LanguagesCacheKey = CacheKeyPrefix + "Languages";
    private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromHours(24);

    public string ProviderName => "DeepL";

    public DeepLProvider(
        IOptions<DeepLSettings> settings,
        ILogger<DeepLProvider> logger,
        IMemoryCache cache)
    {
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;

        // Initialize the translation client
        var authKey = _settings.ApiKey;
        _client = new Translator(authKey, new TranslatorOptions
        {
            ServerUrl = _settings.UseFreeTier ? "https://api-free.deepl.com" : "https://api.deepl.com"
        });

        // Configure retry policy
        _retryPolicy = Policy
            .Handle<Exception>(IsTransientException)
            .WaitAndRetryAsync(
                _settings.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(ex,
                        "Error executing DeepL request. Retry {RetryCount} after {RetryTime}s",
                        retryCount, timeSpan.TotalSeconds);
                });

        // Configure rate limit policy
        _rateLimitPolicy = Policy.RateLimitAsync(60, TimeSpan.FromMinutes(1));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSupportedLanguagesAsync()
    {
        try
        {
            if (_supportedLanguages != null)
                return _supportedLanguages;

            var languages = await ExecuteWithPolicies(() =>
                _client.GetSourceLanguagesAsync());

            _supportedLanguages = languages.Select(l => l.Code.ToLower()).ToList();
            return _supportedLanguages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported languages from DeepL");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        try
        {
            var cacheKey = GetTranslationCacheKey(text, sourceLanguage, targetLanguage);
            if (_cache.TryGetValue(cacheKey, out string? cachedTranslation) && cachedTranslation != null)
                return cachedTranslation;

            var options = new TextTranslateOptions
            {
                PreserveFormatting = _settings.PreserveFormatting == "1",
                Formality = _settings.Formality ? Formality.More : Formality.Default
            };

            var response = await ExecuteWithPolicies(() =>
                _client.TranslateTextAsync(
                    text,
                    sourceLanguage,
                    targetLanguage,
                    options));

            var translation = response.Text;
            _cache.Set(cacheKey, translation, DefaultCacheTime);
            return translation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating text with DeepL. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
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

            var options = new TextTranslateOptions
            {
                PreserveFormatting = _settings.PreserveFormatting == "1",
                Formality = _settings.Formality ? Formality.More : Formality.Default
            };

            // Process in batches according to settings
            for (int i = 0; i < textList.Count; i += _settings.MaxBatchSize)
            {
                var batch = textList.Skip(i).Take(_settings.MaxBatchSize).ToList();
                var responses = await ExecuteWithPolicies(() =>
                    _client.TranslateTextAsync(
                        batch,
                        sourceLanguage,
                        targetLanguage,
                        options));

                var responseList = responses.ToList();
                for (int j = 0; j < responseList.Count; j++)
                {
                    results[textList[i + j]] = responseList[j].Text;
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch translating texts with DeepL. Source: {Source}, Target: {Target}",
                sourceLanguage, targetLanguage);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> DetectLanguageAsync(string text)
    {
        try
        {
            var result = await ExecuteWithPolicies(() =>
                _client.TranslateTextAsync(text, null, "en"));

            return result.DetectedSourceLanguageCode.ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting language with DeepL");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsLanguageSupportedAsync(string languageCode)
    {
        var languages = await GetSupportedLanguagesAsync();
        return languages.Contains(languageCode.ToLower());
    }

    private async Task<T> ExecuteWithPolicies<T>(Func<Task<T>> action)
    {
        return await _retryPolicy.WrapAsync(_rateLimitPolicy)
            .ExecuteAsync(action);
    }

    private static string GetTranslationCacheKey(string text, string sourceLanguage, string targetLanguage)
    {
        return $"{CacheKeyPrefix}{sourceLanguage}_{targetLanguage}_{text.GetHashCode()}";
    }

    private static bool IsTransientException(Exception ex)
    {
        if (ex is DeepLException deepLEx)
        {
            // Check for specific DeepL error types that are transient
            return deepLEx.Message.Contains("quota exceeded") ||
                   deepLEx.Message.Contains("too many requests") ||
                   deepLEx.Message.Contains("resource not found") ||
                   deepLEx.Message.Contains("service unavailable");
        }

        return ex is HttpRequestException || ex is TimeoutException;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}