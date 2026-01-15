using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    private readonly InternationalSettings config;

    public UpdateExchangeRateCommandValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;
        

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.");

        RuleFor(x => x.NewRate)
            .GreaterThanOrEqualTo(0).WithMessage("Döviz kuru 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Source)
            .MaximumLength(50).WithMessage("Kaynak en fazla 50 karakter olabilir.");
    }
}

