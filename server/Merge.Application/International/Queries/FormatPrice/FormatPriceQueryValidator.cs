using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.FormatPrice;

public class FormatPriceQueryValidator(IOptions<InternationalSettings> settings) : AbstractValidator<FormatPriceQuery>
{
    private readonly InternationalSettings config = settings.Value;

    public FormatPriceQueryValidator() : this(Options.Create(new InternationalSettings()))
    {

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Miktar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.");
    }
}

