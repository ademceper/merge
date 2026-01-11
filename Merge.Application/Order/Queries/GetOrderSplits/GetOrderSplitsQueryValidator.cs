using FluentValidation;

namespace Merge.Application.Order.Queries.GetOrderSplits;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetOrderSplitsQueryValidator : AbstractValidator<GetOrderSplitsQuery>
{
    public GetOrderSplitsQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");
    }
}
