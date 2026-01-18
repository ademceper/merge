using FluentValidation;

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

public class RetryFailedPaymentCommandValidator : AbstractValidator<RetryFailedPaymentCommand>
{
    public RetryFailedPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Ödeme ID zorunludur.");
    }
}
