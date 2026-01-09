using FluentValidation;

namespace Merge.Application.International.Queries.FormatPrice;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class FormatPriceQueryValidator : AbstractValidator<FormatPriceQuery>
{
    public FormatPriceQueryValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Miktar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");
    }
}

