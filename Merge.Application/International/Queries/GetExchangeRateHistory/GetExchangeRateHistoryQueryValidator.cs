using FluentValidation;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetExchangeRateHistoryQueryValidator : AbstractValidator<GetExchangeRateHistoryQuery>
{
    public GetExchangeRateHistoryQueryValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365).WithMessage("Gün sayısı 1 ile 365 arasında olmalıdır.");
    }
}

