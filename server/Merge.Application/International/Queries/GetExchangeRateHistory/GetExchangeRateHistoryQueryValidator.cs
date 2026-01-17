using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

public class GetExchangeRateHistoryQueryValidator(IOptions<InternationalSettings> settings) : AbstractValidator<GetExchangeRateHistoryQuery>
{
    private readonly InternationalSettings config = settings.Value;

    public GetExchangeRateHistoryQueryValidator() : this(Options.Create(new InternationalSettings()))
    {

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365).WithMessage("Gün sayısı 1 ile 365 arasında olmalıdır.");
    }
}

