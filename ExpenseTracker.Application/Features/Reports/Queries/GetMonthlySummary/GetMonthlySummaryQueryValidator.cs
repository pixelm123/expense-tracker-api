using FluentValidation;

namespace ExpenseTracker.Application.Features.Reports.Queries.GetMonthlySummary;

public class GetMonthlySummaryQueryValidator : AbstractValidator<GetMonthlySummaryQuery>
{
    public GetMonthlySummaryQueryValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Year must be 2000 or later.");
    }
}
