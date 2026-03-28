using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Reports.Queries.GetMonthlySummary;

public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly IExpenseRepository _expenses;
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public GetMonthlySummaryQueryHandler(
        IExpenseRepository expenses,
        IBudgetRepository budgets,
        ICategoryRepository categories,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _expenses = expenses;
        _budgets = budgets;
        _categories = categories;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"monthly-summary:{request.Month}:{request.Year}:{_currentUser.UserId}";

        var cached = _cache.Get<MonthlySummaryDto>(cacheKey);
        if (cached is not null)
            return cached;

        var result = await BuildSummaryAsync(request.Month, request.Year, cancellationToken);

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    private async Task<MonthlySummaryDto> BuildSummaryAsync(int month, int year, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        var totalsByCategory = await _expenses.GetMonthlyTotalsByCategoryAsync(userId, month, year, ct);
        var budgets = (await _budgets.GetByMonthYearAsync(userId, month, year, ct)).ToDictionary(b => b.CategoryId);
        var categories = await _categories.GetByUserIdAsync(userId, ct);

        var byCategory = categories
            .Where(c => totalsByCategory.ContainsKey(c.Id) || budgets.ContainsKey(c.Id))
            .Select(c =>
            {
                var spent = totalsByCategory.GetValueOrDefault(c.Id, 0m);
                budgets.TryGetValue(c.Id, out var budget);

                return new CategorySummaryDto(
                    CategoryId: c.Id,
                    CategoryName: c.Name,
                    CategoryColor: c.Color,
                    TotalSpent: spent,
                    BudgetLimit: budget?.LimitAmount,
                    AlertThresholdPercentage: budget?.AlertThresholdPercentage,
                    IsAlertThresholdExceeded: budget?.IsAlertThresholdExceeded(spent) ?? false,
                    IsLimitExceeded: budget?.IsLimitExceeded(spent) ?? false,
                    SpendingPercentage: budget?.GetSpendingPercentage(spent));
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToList();

        return new MonthlySummaryDto(
            Month: month,
            Year: year,
            TotalSpending: totalsByCategory.Values.Sum(),
            ByCategory: byCategory);
    }
}
