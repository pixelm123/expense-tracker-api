using FluentValidation;

namespace ExpenseTracker.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Budget limit must be greater than zero.");

        RuleFor(x => x.AlertThresholdPercentage)
            .InclusiveBetween(1, 100).WithMessage("Alert threshold must be between 1 and 100.");
    }
}
