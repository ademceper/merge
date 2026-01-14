using FluentValidation;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RedeemGiftCardCommandValidator : AbstractValidator<RedeemGiftCardCommand>
{
    public RedeemGiftCardCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Hediye kartı kodu zorunludur.")
            .MaximumLength(50).WithMessage("Hediye kartı kodu en fazla 50 karakter olabilir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
