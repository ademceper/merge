using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.GenerateShareCode;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GenerateShareCodeCommandValidator : AbstractValidator<GenerateShareCodeCommand>
{
    public GenerateShareCodeCommandValidator()
    {
        RuleFor(x => x.ComparisonId)
            .NotEmpty().WithMessage("Comparison ID is required");
    }
}
