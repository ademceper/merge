using FluentValidation;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

public class CancelPreOrderCommandValidator : AbstractValidator<CancelPreOrderCommand>
{
    public CancelPreOrderCommandValidator()
    {
        RuleFor(x => x.PreOrderId)
            .NotEmpty().WithMessage("Ön sipariş ID zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    }
}

