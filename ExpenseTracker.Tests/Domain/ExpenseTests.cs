using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Exceptions;
using FluentAssertions;

namespace ExpenseTracker.Tests.Domain;

public class ExpenseTests
{
    private static readonly string UserId = "user-1";
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly DateTime ValidDate = DateTime.UtcNow.AddDays(-1);

    //Construction guards 

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public void Constructor_ShouldThrowDomainException_WhenAmountIsNotPositive(decimal amount)
    {
        var act = () => new Expense(UserId, CategoryId, amount, "Lunch", ValidDate);
        act.Should().Throw<DomainException>().WithMessage("*greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowDomainException_WhenDescriptionIsEmpty(string description)
    {
        var act = () => new Expense(UserId, CategoryId, 50m, description, ValidDate);
        act.Should().Throw<DomainException>().WithMessage("*Description*");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenCategoryIdIsEmpty()
    {
        var act = () => new Expense(UserId, Guid.Empty, 50m, "Lunch", ValidDate);
        act.Should().Throw<DomainException>().WithMessage("*CategoryId*");
    }

    [Fact]
    public void Constructor_ShouldSetAllPropertiesCorrectly_WhenValid()
    {
        var expense = new Expense(UserId, CategoryId, 49.99m, "Coffee", ValidDate);

        expense.UserId.Should().Be(UserId);
        expense.CategoryId.Should().Be(CategoryId);
        expense.Amount.Should().Be(49.99m);
        expense.Description.Should().Be("Coffee");
        expense.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldStoreDate_InUtc()
    {
        var localDate = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Local);

        var expense = new Expense(UserId, CategoryId, 50m, "Lunch", localDate);

        expense.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    //Update

    [Fact]
    public void Update_ShouldChangeAllFields_WhenValid()
    {
        var expense = new Expense(UserId, CategoryId, 50m, "Lunch", ValidDate);
        var newCategoryId = Guid.NewGuid();
        var newDate = DateTime.UtcNow.AddDays(-2);

        expense.Update(99m, "Dinner", newDate, newCategoryId);

        expense.Amount.Should().Be(99m);
        expense.Description.Should().Be("Dinner");
        expense.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void Update_ShouldAdvanceUpdatedAt_WhenCalled()
    {
        var expense = new Expense(UserId, CategoryId, 50m, "Lunch", ValidDate);
        var before = expense.UpdatedAt;

        expense.Update(99m, "Dinner", ValidDate, CategoryId);

        expense.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenAmountIsNegative()
    {
        var expense = new Expense(UserId, CategoryId, 50m, "Lunch", ValidDate);

        var act = () => expense.Update(-10m, "Dinner", ValidDate, CategoryId);

        act.Should().Throw<DomainException>();
    }
}
