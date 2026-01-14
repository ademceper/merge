using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class SetUserCurrencyPreferenceCommandValidator : AbstractValidator<SetUserCurrencyPreferenceCommand>
{
    public SetUserCurrencyPreferenceCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxUserCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxUserCurrencyCodeLength} karakter olmalıdır.");
    }
}

