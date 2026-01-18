using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartByCartItemId;

public class GetCartByCartItemIdQueryValidator : AbstractValidator<GetCartByCartItemIdQuery>
{
    public GetCartByCartItemIdQueryValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Sepet öğesi ID zorunludur");
    }
}

