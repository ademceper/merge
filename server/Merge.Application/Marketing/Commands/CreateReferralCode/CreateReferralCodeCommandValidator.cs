using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateReferralCodeCommandValidator : AbstractValidator<CreateReferralCodeCommand>
{
    public CreateReferralCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
