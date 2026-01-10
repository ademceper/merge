using FluentValidation;

namespace Merge.Application.Logistics.Queries.CalculateShippingCost;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CalculateShippingCostQueryValidator : AbstractValidator<CalculateShippingCostQuery>
{
    public CalculateShippingCostQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");

        RuleFor(x => x.ShippingProvider)
            .NotEmpty().WithMessage("Kargo firması zorunludur.")
            .MaximumLength(100).WithMessage("Kargo firması en fazla 100 karakter olabilir.");
    }
}

