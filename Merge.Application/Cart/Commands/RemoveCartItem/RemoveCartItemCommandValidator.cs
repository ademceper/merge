using FluentValidation;

namespace Merge.Application.Cart.Commands.RemoveCartItem;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Sepet öğesi ID zorunludur");
    }
}

