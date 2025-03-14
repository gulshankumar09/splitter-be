using ExpenseService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseService.API.Services;

public class RecurringExpenseBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringExpenseBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run once a day

    public RecurringExpenseBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecurringExpenseBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Expense Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Processing recurring expenses at: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var recurringExpenseService =
                        scope.ServiceProvider.GetRequiredService<IRecurringExpenseService>();

                    var result = await recurringExpenseService.ProcessDueRecurringExpensesAsync();

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Successfully processed recurring expenses: {message}", result.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to process recurring expenses: {error}", result.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing recurring expenses");
            }

            // Wait for the next execution time
            await Task.Delay(_interval, stoppingToken);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recurring Expense Background Service is starting.");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recurring Expense Background Service is stopping.");
        return base.StopAsync(cancellationToken);
    }
}