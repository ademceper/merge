using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateCurrency;

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    private readonly InternationalSettings config;

    public UpdateCurrencyCommandValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Para birimi ID zorunludur.");

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

