using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.ProcessPayment;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Odeme ID'si zorunludur.");

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Islem ID'si zorunludur.")
            .MaximumLength(200)
            .WithMessage("Islem ID'si en fazla 200 karakter olabilir.");

        RuleFor(x => x.PaymentReference)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.PaymentReference))
            .WithMessage("Odeme referansi en fazla 200 karakter olabilir.");
    }
}
