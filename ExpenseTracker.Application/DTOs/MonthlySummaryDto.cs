namespace ExpenseTracker.Application.DTOs;

public record MonthlySummaryDto(
    int Month,
    int Year,
    decimal TotalSpending,
    IReadOnlyList<CategorySummaryDto> ByCategory);

public record CategorySummaryDto(
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor,
    decimal TotalSpent,
    decimal? BudgetLimit,
    decimal? AlertThresholdPercentage,
    bool IsAlertThresholdExceeded,
    bool IsLimitExceeded,
    decimal? SpendingPercentage);
