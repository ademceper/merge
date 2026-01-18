using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderSplit;

public class GetOrderSplitQueryValidator : AbstractValidator<GetOrderSplitQuery>
{
    public GetOrderSplitQueryValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
