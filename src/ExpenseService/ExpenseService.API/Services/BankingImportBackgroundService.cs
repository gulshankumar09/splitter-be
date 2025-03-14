using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseService.API.Services;

public class BankingImportBackgroundService : BackgroundService
{
    private readonly ILogger<BankingImportBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BankingImportOptions _options;

    // Default to running once per day
    private readonly TimeSpan _defaultImportInterval = TimeSpan.FromHours(24);

    public BankingImportBackgroundService(
        ILogger<BankingImportBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<BankingImportOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Banking Import Background Service started at: {time}", DateTimeOffset.Now);

        // Get the import interval from options, or use default
        var importInterval = _options.ImportIntervalHours > 0
            ? TimeSpan.FromHours(_options.ImportIntervalHours)
            : _defaultImportInterval;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Banking import task running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bankingIntegrationService = scope.ServiceProvider.GetRequiredService<IBankingIntegrationService>();

                    // Import transactions from all active bank accounts
                    var result = await bankingIntegrationService.ImportAllTransactionsAsync();

                    if (result.IsSuccess)
                    {
                        var importedCount = 0;
                        var createdExpenseCount = 0;

                        foreach (var importResult in result.Data)
                        {
                            importedCount += importResult.NewTransactionsImported;
                            createdExpenseCount += importResult.ExpensesCreated;
                        }

                        _logger.LogInformation(
                            "Successfully imported {ImportedCount} transactions and created {CreatedExpenseCount} expenses at: {time}",
                            importedCount, createdExpenseCount, DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to import banking transactions: {error}", result.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during banking transaction import");
            }

            await Task.Delay(importInterval, stoppingToken);
        }

        _logger.LogInformation("Banking Import Background Service stopped at: {time}", DateTimeOffset.Now);
    }
}

public class BankingImportOptions
{
    public int ImportIntervalHours { get; set; } = 24;
    public bool EnableAutoImport { get; set; } = true;
}