using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderSplits;

public class GetOrderSplitsQueryValidator : AbstractValidator<GetOrderSplitsQuery>
{
    public GetOrderSplitsQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");
    }
}
