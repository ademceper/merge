using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

public class GetExchangeRateHistoryQueryValidator(IOptions<InternationalSettings> settings) : AbstractValidator<GetExchangeRateHistoryQuery>
{
    public GetExchangeRateHistoryQueryValidator()
    {
        var config = settings.Value;

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365).WithMessage("Gün sayısı 1 ile 365 arasında olmalıdır.");
    }
}

