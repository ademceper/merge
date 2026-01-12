using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.ML.Commands.OptimizePrice;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class OptimizePriceCommandValidator : AbstractValidator<OptimizePriceCommand>
{
    public OptimizePriceCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.MinPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum price must be greater than or equal to 0.")
                .When(x => x.Request!.MinPrice.HasValue);

            RuleFor(x => x.Request!.MaxPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum price must be greater than or equal to 0.")
                .When(x => x.Request!.MaxPrice.HasValue);

            RuleFor(x => x.Request!.Strategy)
                .MaximumLength(100).WithMessage("Strategy cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Request!.Strategy));
        });
    }
}
