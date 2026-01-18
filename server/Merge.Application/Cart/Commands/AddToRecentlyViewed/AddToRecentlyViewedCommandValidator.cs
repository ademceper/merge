using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.AddToRecentlyViewed;

public class AddToRecentlyViewedCommandValidator : AbstractValidator<AddToRecentlyViewedCommand>
{
    public AddToRecentlyViewedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur");
    }
}

