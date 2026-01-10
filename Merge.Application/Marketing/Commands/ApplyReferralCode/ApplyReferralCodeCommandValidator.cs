using FluentValidation;

namespace Merge.Application.Marketing.Commands.ApplyReferralCode;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ApplyReferralCodeCommandValidator : AbstractValidator<ApplyReferralCodeCommand>
{
    public ApplyReferralCodeCommandValidator()
    {
        RuleFor(x => x.NewUserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Referans kodu zorunludur.")
            .MaximumLength(50).WithMessage("Referans kodu en fazla 50 karakter olabilir.");
    }
}
