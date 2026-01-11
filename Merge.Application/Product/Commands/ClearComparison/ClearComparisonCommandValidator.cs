using FluentValidation;

namespace Merge.Application.Product.Commands.ClearComparison;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ClearComparisonCommandValidator : AbstractValidator<ClearComparisonCommand>
{
    public ClearComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
