using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.CreatePayment;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Siparis ID'si zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Odeme tutari 0'dan buyuk olmalidir.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Odeme yontemi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Odeme yontemi en fazla 50 karakter olabilir.");

        RuleFor(x => x.PaymentProvider)
            .NotEmpty()
            .WithMessage("Odeme saglayicisi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Odeme saglayicisi en fazla 50 karakter olabilir.");
    }
}
