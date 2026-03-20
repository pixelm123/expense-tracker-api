using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using FluentAssertions;

namespace ExpenseTracker.Tests.Domain;

public class BudgetTests
{
    private static readonly string UserId = "user-1";
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Constructor_ShouldThrowDomainException_WhenMonthIsOutOfRange(int month)
    {
        var act = () => new Budget(UserId, CategoryId, 500m, month, 2025);
        act.Should().Throw<DomainException>().WithMessage("*Month*");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenLimitAmountIsZero()
    {
        var act = () => new Budget(UserId, CategoryId, 0m, 3, 2025);
        act.Should().Throw<DomainException>().WithMessage("*limit*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-5)]
    public void Constructor_ShouldThrowDomainException_WhenAlertThresholdIsOutOfRange(decimal threshold)
    {
        var act = () => new Budget(UserId, CategoryId, 500m, 3, 2025, threshold);
        act.Should().Throw<DomainException>().WithMessage("*threshold*");
    }

    [Fact]
    public void Constructor_ShouldSetDefaultAlertThresholdTo80_WhenNotSpecified()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        budget.AlertThresholdPercentage.Should().Be(80m);
    }

    [Fact]
    public void IsAlertThresholdExceeded_ShouldReturnTrue_WhenSpendingReachesThreshold()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025, alertThresholdPercentage: 80);
        budget.IsAlertThresholdExceeded(800m).Should().BeTrue();
    }

    [Fact]
    public void IsAlertThresholdExceeded_ShouldReturnTrue_WhenSpendingExceedsThreshold()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025, alertThresholdPercentage: 80);
        budget.IsAlertThresholdExceeded(950m).Should().BeTrue();
    }

    [Fact]
    public void IsAlertThresholdExceeded_ShouldReturnFalse_WhenSpendingIsBelowThreshold()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025, alertThresholdPercentage: 80);
        budget.IsAlertThresholdExceeded(799m).Should().BeFalse();
    }

    [Fact]
    public void IsLimitExceeded_ShouldReturnTrue_WhenSpendingEqualsLimit()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        budget.IsLimitExceeded(1000m).Should().BeTrue();
    }

    [Fact]
    public void IsLimitExceeded_ShouldReturnFalse_WhenSpendingIsBelowLimit()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        budget.IsLimitExceeded(999.99m).Should().BeFalse();
    }

    [Fact]
    public void GetSpendingPercentage_ShouldReturnCorrectPercentage()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        budget.GetSpendingPercentage(750m).Should().Be(75m);
    }

    [Fact]
    public void GetSpendingPercentage_ShouldCapAt100_WhenSpendingExceedsLimit()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        budget.GetSpendingPercentage(1500m).Should().Be(100m);
    }

    [Fact]
    public void Update_ShouldChangeFields_WhenValid()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025, alertThresholdPercentage: 80);
        budget.Update(2000m, 90m);
        budget.LimitAmount.Should().Be(2000m);
        budget.AlertThresholdPercentage.Should().Be(90m);
    }

    [Fact]
    public void Update_ShouldAdvanceUpdatedAt_WhenCalled()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        var before = budget.UpdatedAt;
        budget.Update(2000m, 80m);
        budget.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenNewLimitIsNegative()
    {
        var budget = new Budget(UserId, CategoryId, 1000m, 3, 2025);
        var act = () => budget.Update(-100m, 80m);
        act.Should().Throw<DomainException>();
    }
}
