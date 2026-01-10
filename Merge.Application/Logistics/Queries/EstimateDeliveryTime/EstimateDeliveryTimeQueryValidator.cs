using FluentValidation;

namespace Merge.Application.Logistics.Queries.EstimateDeliveryTime;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class EstimateDeliveryTimeQueryValidator : AbstractValidator<EstimateDeliveryTimeQuery>
{
    public EstimateDeliveryTimeQueryValidator()
    {
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke zorunludur.")
            .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Sipariş tarihi zorunludur.");
    }
}

