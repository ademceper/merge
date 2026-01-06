using FluentValidation;

namespace Merge.Application.Payment.Commands.RefundPayment;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Odeme ID'si zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("Iade tutari 0'dan buyuk olmalidir.");
    }
}
