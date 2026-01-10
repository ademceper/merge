using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetPickPacksByOrderId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetPickPacksByOrderIdQueryValidator : AbstractValidator<GetPickPacksByOrderIdQuery>
{
    public GetPickPacksByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");
    }
}

