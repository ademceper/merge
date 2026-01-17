using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

public class SetUserCurrencyPreferenceCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<SetUserCurrencyPreferenceCommand>
{
    private readonly InternationalSettings config = settings.Value;

    public SetUserCurrencyPreferenceCommandValidator() : this(Options.Create(new InternationalSettings()))
    {
        

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxUserCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxUserCurrencyCodeLength} karakter olmalıdır.");
    }
}

