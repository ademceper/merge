using FluentValidation;

namespace Merge.Application.Payment.Commands.SetDefaultPaymentMethod;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class SetDefaultPaymentMethodCommandValidator : AbstractValidator<SetDefaultPaymentMethodCommand>
{
    public SetDefaultPaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Ödeme yöntemi ID'si zorunludur.");
    }
}
