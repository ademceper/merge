using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrder;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");
    }
}
