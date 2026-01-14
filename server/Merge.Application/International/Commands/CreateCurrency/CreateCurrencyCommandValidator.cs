using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.CreateCurrency;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength).WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Para birimi adı zorunludur.")
            .MaximumLength(config.MaxCurrencyNameLength).WithMessage($"Para birimi adı en fazla {config.MaxCurrencyNameLength} karakter olabilir.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol zorunludur.")
            .MaximumLength(config.MaxCurrencySymbolLength).WithMessage($"Sembol en fazla {config.MaxCurrencySymbolLength} karakter olabilir.");

        RuleFor(x => x.ExchangeRate)
            .GreaterThanOrEqualTo(0).WithMessage("Döviz kuru 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(config.MinCurrencyDecimalPlaces, config.MaxCurrencyDecimalPlaces)
            .WithMessage($"Ondalık basamak sayısı {config.MinCurrencyDecimalPlaces} ile {config.MaxCurrencyDecimalPlaces} arasında olmalıdır.");

        RuleFor(x => x.Format)
            .MaximumLength(config.MaxCurrencyFormatLength).WithMessage($"Format en fazla {config.MaxCurrencyFormatLength} karakter olabilir.");
    }
}

