using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class RecurringExpenseJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RecurringExpenseJob> _logger;

    public RecurringExpenseJob(IServiceProvider services, ILogger<RecurringExpenseJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringExpenseJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Run once per day at midnight
            var nextRun = now.Date.AddDays(1);
            var delay = nextRun - now;

            _logger.LogInformation("RecurringExpenseJob: next run in {Hours}h {Minutes}m", 
                (int)delay.TotalHours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _services.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IRecurringExpenseService>();
                await service.ProcessDueRecurringExpensesAsync();
                _logger.LogInformation("RecurringExpenseJob: processed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RecurringExpenseJob failed: {Message}", ex.Message);
            }
        }
    }
}
