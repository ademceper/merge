using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.MoveToCart;

public class MoveToCartCommandValidator : AbstractValidator<MoveToCartCommand>
{
    public MoveToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Öğe ID zorunludur");
    }
}

