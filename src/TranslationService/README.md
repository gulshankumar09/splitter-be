# Translation Service

A robust .NET translation service that supports multiple translation providers with fallback capabilities and caching.

## Features

- Support for multiple translation providers:
  - Azure Translator
  - Google Cloud Translation
  - DeepL
  - LibreTranslate
- Automatic fallback between providers
- In-memory caching for improved performance
- Batch translation support
- Language detection
- Configurable retry policies and rate limiting
- Comprehensive error handling and logging

## Configuration

The service can be configured through `appsettings.json`:

```json
{
  "TranslationService": {
    "DefaultProvider": "Google",
    "EnableFallback": true,
    "FallbackOrder": ["Google", "Azure", "DeepL", "LibreTranslate"],
    "ShareCache": true,
    "CacheDurationHours": 24,
    "MaxCacheSizeMB": 1024
  }
}
```

### Provider-Specific Settings

#### Azure Translator

```json
{
  "AzureTranslate": {
    "SubscriptionKey": "your-subscription-key",
    "Endpoint": "https://api.cognitive.microsofttranslator.com/",
    "Region": "your-region",
    "TimeoutSeconds": 30,
    "MaxBatchSize": 100,
    "EnableRetries": true,
    "MaxRetries": 3,
    "UsePremiumTier": false,
    "IncludeSentenceLength": false,
    "IncludeAlignment": false,
    "IncludeSourceText": false,
    "FilterProfanity": false,
    "Category": "general"
  }
}
```

#### Google Cloud Translation

```json
{
  "GoogleTranslate": {
    "ProjectId": "your-project-id",
    "CredentialsPath": "path/to/credentials.json",
    "CredentialsJson": null,
    "Location": "global",
    "UsePremiumModel": false,
    "TimeoutSeconds": 30,
    "MaxBatchSize": 100,
    "EnableRetries": true,
    "MaxRetries": 3
  }
}
```

#### DeepL

```json
{
  "DeepL": {
    "ApiKey": "your-api-key",
    "UseFreeTier": false,
    "TimeoutSeconds": 30,
    "MaxBatchSize": 50,
    "EnableRetries": true,
    "MaxRetries": 3,
    "SplitSentences": "1",
    "PreserveFormatting": "0",
    "Formality": false,
    "TagHandling": null,
    "IgnoreTags": null,
    "UseGlossary": false,
    "GlossaryId": null
  }
}
```

#### LibreTranslate

```json
{
  "LibreTranslate": {
    "ApiUrl": "https://libretranslate.com",
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30,
    "MaxBatchSize": 50,
    "EnableRetries": true,
    "MaxRetries": 3,
    "UseHttps": true,
    "VerifySsl": true,
    "PreferFast": false,
    "IncludeSourceText": false,
    "IncludeConfidence": false
  }
}
```

## Usage

### Dependency Injection Setup

```csharp
services.AddTranslationService(configuration);
```

### Basic Usage

```csharp
public class YourService
{
    private readonly ITranslationProvider _translationProvider;

    public YourService(ITranslationProvider translationProvider)
    {
        _translationProvider = translationProvider;
    }

    public async Task<string> TranslateText(string text, string targetLanguage)
    {
        // Detect source language
        var sourceLanguage = await _translationProvider.DetectLanguageAsync(text);

        // Translate text
        return await _translationProvider.TranslateAsync(text, sourceLanguage, targetLanguage);
    }

    public async Task<IDictionary<string, string>> TranslateBatch(
        IEnumerable<string> texts,
        string targetLanguage)
    {
        // Detect source language from first text
        var sourceLanguage = await _translationProvider.DetectLanguageAsync(texts.First());

        // Translate batch
        return await _translationProvider.TranslateBatchAsync(texts, sourceLanguage, targetLanguage);
    }
}
```

## Features in Detail

### Fallback Mechanism

The service automatically falls back to alternative providers if the primary provider fails. The fallback order is configurable through settings.

### Caching

- In-memory caching for translations
- Configurable cache duration
- Cache size limits
- Shared cache across providers when enabled

### Error Handling

- Automatic retry for transient errors
- Rate limiting protection
- Comprehensive logging
- Fallback to alternative providers on failure

### Performance

- Batch translation support
- Configurable timeouts
- Rate limiting
- Caching for improved response times

## Requirements

- .NET 6.0 or later
- Valid API keys for the translation providers you want to use
- Appropriate network access to the translation service endpoints

## Dependencies

- Azure.AI.Translation.Text
- Google.Cloud.Translation.V2
- DeepL
- Polly (for retry and rate limiting policies)
- Microsoft.Extensions.Caching.Memory
- Microsoft.Extensions.Options

## License

[Your License Here]
