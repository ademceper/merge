using FluentValidation;

namespace Merge.Application.Subscription.Commands.ProcessPayment;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Ödeme ID zorunludur.");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("İşlem ID zorunludur.")
            .MaximumLength(200).WithMessage("İşlem ID en fazla 200 karakter olabilir.");
    }
}
