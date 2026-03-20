using FluentValidation;

namespace ExpenseTracker.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Budget limit must be greater than zero.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Year must be 2000 or later.");

        RuleFor(x => x.AlertThresholdPercentage)
            .InclusiveBetween(1, 100).WithMessage("Alert threshold must be between 1 and 100.");
    }
}
