using FluentValidation;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class SetUserLanguagePreferenceCommandValidator : AbstractValidator<SetUserLanguagePreferenceCommand>
{
    public SetUserLanguagePreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");
    }
}

