using FluentValidation;

namespace Merge.Application.Cart.Commands.ConvertPreOrderToOrder;

public class ConvertPreOrderToOrderCommandValidator : AbstractValidator<ConvertPreOrderToOrderCommand>
{
    public ConvertPreOrderToOrderCommandValidator()
    {
        RuleFor(x => x.PreOrderId)
            .NotEmpty().WithMessage("Ön sipariş ID zorunludur.");
    }
}

