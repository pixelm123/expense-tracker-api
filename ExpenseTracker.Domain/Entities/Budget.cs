using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.Domain.Entities;

public class Budget : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public decimal LimitAmount { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public decimal AlertThresholdPercentage { get; private set; }

    private Budget() { }

    public Budget(
        string userId,
        Guid categoryId,
        decimal limitAmount,
        int month,
        int year,
        decimal alertThresholdPercentage = 80)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId is required.");
        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId is required.");
        if (limitAmount <= 0)
            throw new DomainException("Budget limit must be greater than zero.");
        if (month < 1 || month > 12)
            throw new DomainException("Month must be between 1 and 12.");
        if (year < 2000)
            throw new DomainException("Year is not valid.");
        if (alertThresholdPercentage is <= 0 or > 100)
            throw new DomainException("Alert threshold must be between 1 and 100.");

        UserId = userId;
        CategoryId = categoryId;
        LimitAmount = limitAmount;
        Month = month;
        Year = year;
        AlertThresholdPercentage = alertThresholdPercentage;
    }

    public bool IsAlertThresholdExceeded(decimal currentSpending)
        => currentSpending >= LimitAmount * AlertThresholdPercentage / 100m;

    public bool IsLimitExceeded(decimal currentSpending)
        => currentSpending >= LimitAmount;

    public decimal GetSpendingPercentage(decimal currentSpending)
        => Math.Min(currentSpending / LimitAmount * 100m, 100m);

    public void Update(decimal limitAmount, decimal alertThresholdPercentage)
    {
        if (limitAmount <= 0)
            throw new DomainException("Budget limit must be greater than zero.");
        if (alertThresholdPercentage is <= 0 or > 100)
            throw new DomainException("Alert threshold must be between 1 and 100.");

        LimitAmount = limitAmount;
        AlertThresholdPercentage = alertThresholdPercentage;
        Touch();
    }
}
