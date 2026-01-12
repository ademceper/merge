using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Product.Commands.DeleteComparison;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class DeleteComparisonCommandValidator : AbstractValidator<DeleteComparisonCommand>
{
    public DeleteComparisonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comparison ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
