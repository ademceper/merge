using FluentValidation;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Sepet öğesi ID zorunludur");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır");
    }
}

