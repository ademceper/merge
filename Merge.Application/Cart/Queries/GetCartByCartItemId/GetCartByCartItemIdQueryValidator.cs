using FluentValidation;

namespace Merge.Application.Cart.Queries.GetCartByCartItemId;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCartByCartItemIdQueryValidator : AbstractValidator<GetCartByCartItemIdQuery>
{
    public GetCartByCartItemIdQueryValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Sepet öğesi ID zorunludur");
    }
}

