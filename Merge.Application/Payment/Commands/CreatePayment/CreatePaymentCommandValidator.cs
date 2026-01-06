using FluentValidation;

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
            .IsInEnum()
            .WithMessage("Gecerli bir odeme yontemi secilmelidir.");

        RuleFor(x => x.PaymentProvider)
            .IsInEnum()
            .WithMessage("Gecerli bir odeme saglayicisi secilmelidir.");
    }
}
