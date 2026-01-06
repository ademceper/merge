using FluentValidation;

namespace Merge.Application.Cart.Commands.SendRecoveryEmail;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SendRecoveryEmailCommandValidator : AbstractValidator<SendRecoveryEmailCommand>
{
    public SendRecoveryEmailCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");

        RuleFor(x => x.CouponDiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.CouponDiscountPercentage.HasValue)
            .WithMessage("Kupon indirim yüzdesi 0 ile 100 arasında olmalıdır");
    }
}

