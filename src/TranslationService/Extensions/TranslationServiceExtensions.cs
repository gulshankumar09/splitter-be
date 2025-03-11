using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TranslationService.Configuration;
using TranslationService.Enums;
using TranslationService.Interfaces;
using TranslationService.Services;
using TranslationService.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace TranslationService.Extensions;

/// <summary>
/// Extension methods for configuring translation services
/// </summary>
public static class TranslationServiceExtensions
{
    /// <summary>
    /// Adds translation services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTranslationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register settings
        services.Configure<TranslationServiceSettings>(
            configuration.GetSection(TranslationServiceSettings.SectionName));

        // Register database context and repository
        services.AddDbContext<TranslationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITranslationRepository, TranslationRepository>();

        // Register all providers
        services.AddGoogleTranslate(configuration);
        services.AddAzureTranslate(configuration);
        services.AddDeepL(configuration);
        services.AddLibreTranslate(configuration);

        // Register factory
        services.AddSingleton<ITranslationProviderFactory, TranslationProviderFactory>();

        // Register resilient providers for each provider type
        foreach (TranslationProvider providerType in Enum.GetValues(typeof(TranslationProvider)))
        {
            services.AddSingleton<ITranslationProvider>(sp =>
                new ResilientTranslationProvider(
                    sp.GetRequiredService<ITranslationProviderFactory>(),
                    sp.GetRequiredService<ILogger<ResilientTranslationProvider>>(),
                    sp.GetRequiredService<IOptions<TranslationServiceSettings>>(),
                    providerType,
                    sp.GetRequiredService<ITranslationRepository>()));
        }

        return services;
    }

    /// <summary>
    /// Adds translation services to the service collection with selective provider registration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="useGoogle">Whether to register Google Translate</param>
    /// <param name="useAzure">Whether to register Azure Translator</param>
    /// <param name="useDeepL">Whether to register DeepL</param>
    /// <param name="useLibreTranslate">Whether to register LibreTranslate</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTranslationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useGoogle = true,
        bool useAzure = true,
        bool useDeepL = true,
        bool useLibreTranslate = true)
    {
        // Register settings
        services.Configure<TranslationServiceSettings>(
            configuration.GetSection(TranslationServiceSettings.SectionName));

        // Register database context and repository
        services.AddDbContext<TranslationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITranslationRepository, TranslationRepository>();

        // Register selected providers
        if (useGoogle)
            services.AddGoogleTranslate(configuration);
        if (useAzure)
            services.AddAzureTranslate(configuration);
        if (useDeepL)
            services.AddDeepL(configuration);
        if (useLibreTranslate)
            services.AddLibreTranslate(configuration);

        // Register factory
        services.AddSingleton<ITranslationProviderFactory, TranslationProviderFactory>();

        // Register resilient providers only for enabled provider types
        var enabledProviders = new List<TranslationProvider>();
        if (useGoogle) enabledProviders.Add(TranslationProvider.Google);
        if (useAzure) enabledProviders.Add(TranslationProvider.Azure);
        if (useDeepL) enabledProviders.Add(TranslationProvider.DeepL);
        if (useLibreTranslate) enabledProviders.Add(TranslationProvider.LibreTranslate);

        foreach (var providerType in enabledProviders)
        {
            services.AddSingleton<ITranslationProvider>(sp =>
                new ResilientTranslationProvider(
                    sp.GetRequiredService<ITranslationProviderFactory>(),
                    sp.GetRequiredService<ILogger<ResilientTranslationProvider>>(),
                    sp.GetRequiredService<IOptions<TranslationServiceSettings>>(),
                    providerType,
                    sp.GetRequiredService<ITranslationRepository>()));
        }

        return services;
    }
}