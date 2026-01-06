using FluentValidation;

namespace Merge.Application.Cart.Commands.RemoveFromWishlist;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RemoveFromWishlistCommandValidator : AbstractValidator<RemoveFromWishlistCommand>
{
    public RemoveFromWishlistCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur");
    }
}

