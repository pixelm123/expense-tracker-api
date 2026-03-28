using ExpenseTracker.Application.Common.Interfaces;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Features.Reports.Queries.GetMonthlySummary;
using ExpenseTracker.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests.Application.Reports;

public class GetMonthlySummaryQueryHandlerTests
{
    private readonly Mock<IExpenseRepository> _expenses = new();
    private readonly Mock<IBudgetRepository> _budgets = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly GetMonthlySummaryQueryHandler _handler;

    private const string UserId = "user-1";
    private const int Month = 3;
    private const int Year = 2026;

    public GetMonthlySummaryQueryHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _handler = new GetMonthlySummaryQueryHandler(
            _expenses.Object, _budgets.Object, _categories.Object,
            _currentUser.Object, _cache.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedResult_WhenCacheHit()
    {
        var cached = new MonthlySummaryDto(Month, Year, 500m, []);
        _cache.Setup(c => c.Get<MonthlySummaryDto>(It.IsAny<string>())).Returns(cached);

        var result = await _handler.Handle(new GetMonthlySummaryQuery(Month, Year), CancellationToken.None);

        result.Should().BeSameAs(cached);

        _expenses.Verify(r => r.GetMonthlyTotalsByCategoryAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _budgets.Verify(r => r.GetByMonthYearAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldQueryDatabaseAndCacheResult_WhenCacheMiss()
    {
        var category = new Category("Food", UserId, "#FF5733");
        SetupCacheMiss();
        SetupRepositories(
            categories: [category],
            totals: new Dictionary<Guid, decimal> { [category.Id] = 750m },
            budgets: []);

        var result = await _handler.Handle(new GetMonthlySummaryQuery(Month, Year), CancellationToken.None);

        result.TotalSpending.Should().Be(750m);
        result.ByCategory.Should().HaveCount(1);
        result.ByCategory[0].CategoryName.Should().Be("Food");
        result.ByCategory[0].TotalSpent.Should().Be(750m);

        _cache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<MonthlySummaryDto>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCorrectlyEvaluateBudgetAlertThreshold_WhenSpendingCrossesThreshold()
    {
        var category = new Category("Food", UserId);
        var budget = new Budget(UserId, category.Id, 1000m, Month, Year, alertThresholdPercentage: 80);

        SetupCacheMiss();
        SetupRepositories(
            categories: [category],
            totals: new Dictionary<Guid, decimal> { [category.Id] = 850m },
            budgets: [budget]);

        var result = await _handler.Handle(new GetMonthlySummaryQuery(Month, Year), CancellationToken.None);

        var summary = result.ByCategory.Single();
        summary.IsAlertThresholdExceeded.Should().BeTrue(because: "850 >= 80% of 1000 (800)");
        summary.IsLimitExceeded.Should().BeFalse(because: "850 < 1000");
        summary.SpendingPercentage.Should().Be(85m);
        summary.BudgetLimit.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_ShouldMarkLimitExceeded_WhenSpendingExceedsLimit()
    {
        var category = new Category("Food", UserId);
        var budget = new Budget(UserId, category.Id, 500m, Month, Year, alertThresholdPercentage: 80);

        SetupCacheMiss();
        SetupRepositories(
            categories: [category],
            totals: new Dictionary<Guid, decimal> { [category.Id] = 600m },
            budgets: [budget]);

        var result = await _handler.Handle(new GetMonthlySummaryQuery(Month, Year), CancellationToken.None);

        var summary = result.ByCategory.Single();
        summary.IsLimitExceeded.Should().BeTrue();
        summary.SpendingPercentage.Should().Be(100m, because: "GetSpendingPercentage caps at 100");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptySummary_WhenUserHasNoExpensesOrBudgets()
    {
        SetupCacheMiss();
        SetupRepositories(categories: [], totals: [], budgets: []);

        var result = await _handler.Handle(new GetMonthlySummaryQuery(Month, Year), CancellationToken.None);

        result.TotalSpending.Should().Be(0m);
        result.ByCategory.Should().BeEmpty();
    }

    private void SetupCacheMiss()
        => _cache.Setup(c => c.Get<MonthlySummaryDto>(It.IsAny<string>()))
            .Returns((MonthlySummaryDto?)null);

    private void SetupRepositories(List<Category> categories, Dictionary<Guid, decimal> totals, List<Budget> budgets)
    {
        _categories.Setup(r => r.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(categories);
        _expenses.Setup(r => r.GetMonthlyTotalsByCategoryAsync(UserId, Month, Year, It.IsAny<CancellationToken>())).ReturnsAsync(totals);
        _budgets.Setup(r => r.GetByMonthYearAsync(UserId, Month, Year, It.IsAny<CancellationToken>())).ReturnsAsync(budgets);
    }
}
