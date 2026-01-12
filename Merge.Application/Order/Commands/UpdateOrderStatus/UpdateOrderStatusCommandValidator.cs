using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.UpdateOrderStatus;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Geçersiz sipariş durumu.");
    }
}
