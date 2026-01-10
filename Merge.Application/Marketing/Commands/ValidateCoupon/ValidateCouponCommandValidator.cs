using FluentValidation;

namespace Merge.Application.Marketing.Commands.ValidateCoupon;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ValidateCouponCommandValidator : AbstractValidator<ValidateCouponCommand>
{
    public ValidateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kupon kodu zorunludur.")
            .MaximumLength(50).WithMessage("Kupon kodu en fazla 50 karakter olabilir.");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Sipariş tutarı 0'dan büyük olmalıdır.");
    }
}
