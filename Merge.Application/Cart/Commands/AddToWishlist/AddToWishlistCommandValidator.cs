using FluentValidation;

namespace Merge.Application.Cart.Commands.AddToWishlist;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur");
    }
}

