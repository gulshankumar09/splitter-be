using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseService.API.Services;

public class BudgetBackgroundService : BackgroundService
{
    private readonly ILogger<BudgetBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    public BudgetBackgroundService(
        ILogger<BudgetBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Budget Background Service started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Budget processing task running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();

                    // Check budget thresholds
                    var thresholdResult = await budgetService.CheckBudgetThresholdsAsync();
                    if (thresholdResult.IsSuccess)
                    {
                        _logger.LogInformation("Successfully checked budget thresholds at: {time}", DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to check budget thresholds: {error}", thresholdResult.Error);
                    }

                    // Renew recurring budgets
                    var renewalResult = await budgetService.RenewRecurringBudgetsAsync();
                    if (renewalResult.IsSuccess)
                    {
                        _logger.LogInformation("Successfully renewed recurring budgets at: {time}", DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to renew recurring budgets: {error}", renewalResult.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during budget processing");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Budget Background Service stopped at: {time}", DateTimeOffset.Now);
    }
}