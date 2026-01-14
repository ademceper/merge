using FluentValidation;

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class RetryFailedPaymentCommandValidator : AbstractValidator<RetryFailedPaymentCommand>
{
    public RetryFailedPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Ödeme ID zorunludur.");
    }
}
