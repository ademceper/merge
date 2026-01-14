using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingByOrderId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetShippingByOrderIdQueryValidator : AbstractValidator<GetShippingByOrderIdQuery>
{
    public GetShippingByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");
    }
}

