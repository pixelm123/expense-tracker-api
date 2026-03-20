using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Infrastructure.BackgroundServices;

public class BudgetAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BudgetAlertBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public BudgetAlertBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BudgetAlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Budget alert service started. Checking every {Interval}.", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckBudgetsAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CheckBudgetsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var budgets = await db.Budgets
                .Include(b => b.Category)
                .Where(b => b.Month == now.Month && b.Year == now.Year)
                .ToListAsync(ct);

            if (budgets.Count == 0)
                return;

            var userIds = budgets.Select(b => b.UserId).Distinct().ToList();

            var spending = await db.Expenses
                .Where(e => userIds.Contains(e.UserId)
                         && e.Date >= monthStart
                         && e.Date < monthEnd)
                .GroupBy(e => new { e.UserId, e.CategoryId })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.CategoryId,
                    Total = g.Sum(e => e.Amount),
                })
                .ToListAsync(ct);

            var spendingLookup = spending.ToDictionary(
                s => (s.UserId, s.CategoryId),
                s => s.Total);

            foreach (var budget in budgets)
            {
                var spent = spendingLookup.GetValueOrDefault((budget.UserId, budget.CategoryId), 0m);

                if (budget.IsAlertThresholdExceeded(spent))
                {
                    _logger.LogWarning(
                        "Budget alert | User: {UserId} | Category: '{Category}' | " +
                        "Spent: {Spent:F2} / Limit: {Limit:F2} ({Pct:F1}%) | Month: {Month}/{Year}",
                        budget.UserId,
                        budget.Category.Name,
                        spent,
                        budget.LimitAmount,
                        budget.GetSpendingPercentage(spent),
                        budget.Month,
                        budget.Year);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Budget alert check failed.");
        }
    }
}
