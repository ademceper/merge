using FluentValidation;

namespace Merge.Application.Product.Commands.SaveComparison;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class SaveComparisonCommandValidator : AbstractValidator<SaveComparisonCommand>
{
    public SaveComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }
}
