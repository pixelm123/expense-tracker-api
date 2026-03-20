using FluentValidation;

namespace ExpenseTracker.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Color)
            .Matches(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")
            .WithMessage("Color must be a valid hex code (e.g. #FF5733).")
            .When(x => x.Color is not null);
    }
}
