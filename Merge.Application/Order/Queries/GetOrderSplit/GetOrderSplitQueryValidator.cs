using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderSplit;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetOrderSplitQueryValidator : AbstractValidator<GetOrderSplitQuery>
{
    public GetOrderSplitQueryValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
