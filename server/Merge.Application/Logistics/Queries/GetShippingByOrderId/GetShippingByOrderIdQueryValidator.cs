using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingByOrderId;

public class GetShippingByOrderIdQueryValidator : AbstractValidator<GetShippingByOrderIdQuery>
{
    public GetShippingByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");
    }
}

