using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ExpenseTracker.Tests.Application.Expenses;

public class CreateExpenseCommandValidatorTests
{
    private readonly CreateExpenseCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenAmountIsNotPositive(decimal amount)
    {
        var command = ValidCommand() with { Amount = amount };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDescriptionIsEmpty()
    {
        var command = ValidCommand() with { Description = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCategoryIdIsEmpty()
    {
        var command = ValidCommand() with { CategoryId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateIsInTheFuture()
    {
        var command = ValidCommand() with { Date = DateTime.UtcNow.AddDays(1) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Date);
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateExpenseCommand ValidCommand() =>
        new(Amount: 49.99m, Description: "Coffee", Date: DateTime.UtcNow.AddDays(-1), CategoryId: Guid.NewGuid());
}
