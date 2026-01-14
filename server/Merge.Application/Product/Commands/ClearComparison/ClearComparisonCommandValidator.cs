using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

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
