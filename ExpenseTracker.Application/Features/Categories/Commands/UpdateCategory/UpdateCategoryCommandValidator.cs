using FluentValidation;

namespace ExpenseTracker.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Color)
            .Matches(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")
            .WithMessage("Color must be a valid hex code (e.g. #FF5733).")
            .When(x => x.Color is not null);
    }
}
