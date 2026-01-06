using FluentValidation;

namespace Merge.Application.Cart.Commands.NotifyPreOrderAvailable;

public class NotifyPreOrderAvailableCommandValidator : AbstractValidator<NotifyPreOrderAvailableCommand>
{
    public NotifyPreOrderAvailableCommandValidator()
    {
        RuleFor(x => x.PreOrderId)
            .NotEmpty().WithMessage("Ön sipariş ID zorunludur.");
    }
}

