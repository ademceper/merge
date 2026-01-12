using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetSplitOrders;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetSplitOrdersQueryValidator : AbstractValidator<GetSplitOrdersQuery>
{
    public GetSplitOrdersQueryValidator()
    {
        RuleFor(x => x.SplitOrderId)
            .NotEmpty()
            .WithMessage("Bölünmüş sipariş ID'si zorunludur.");
    }
}
