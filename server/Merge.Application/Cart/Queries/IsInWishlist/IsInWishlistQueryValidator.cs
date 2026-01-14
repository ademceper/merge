using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.IsInWishlist;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class IsInWishlistQueryValidator : AbstractValidator<IsInWishlistQuery>
{
    public IsInWishlistQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur");
    }
}

