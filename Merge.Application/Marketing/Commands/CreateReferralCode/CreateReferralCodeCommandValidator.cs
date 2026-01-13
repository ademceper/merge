using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateReferralCodeCommandValidator : AbstractValidator<CreateReferralCodeCommand>
{
    public CreateReferralCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
