using FluentValidation;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class SetUserCurrencyPreferenceCommandValidator : AbstractValidator<SetUserCurrencyPreferenceCommand>
{
    public SetUserCurrencyPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");
    }
}

