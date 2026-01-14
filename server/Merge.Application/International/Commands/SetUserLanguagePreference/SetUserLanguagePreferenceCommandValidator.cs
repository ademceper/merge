using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class SetUserLanguagePreferenceCommandValidator : AbstractValidator<SetUserLanguagePreferenceCommand>
{
    public SetUserLanguagePreferenceCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxUserLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxUserLanguageCodeLength} karakter olmalıdır.");
    }
}

