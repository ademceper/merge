using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.DeletePaymentMethod;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeletePaymentMethodCommandValidator : AbstractValidator<DeletePaymentMethodCommand>
{
    public DeletePaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Ödeme yöntemi ID'si zorunludur.");
    }
}
