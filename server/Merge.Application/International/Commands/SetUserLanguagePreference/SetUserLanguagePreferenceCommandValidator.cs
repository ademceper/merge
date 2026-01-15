using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

public class SetUserLanguagePreferenceCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<SetUserLanguagePreferenceCommand>
{
    public SetUserLanguagePreferenceCommandValidator()
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

