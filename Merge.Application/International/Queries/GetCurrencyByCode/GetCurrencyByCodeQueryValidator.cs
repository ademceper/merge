using FluentValidation;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetCurrencyByCodeQueryValidator : AbstractValidator<GetCurrencyByCodeQuery>
{
    public GetCurrencyByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.")
            .Matches("^[A-Z]{3}$").WithMessage("Para birimi kodu 3 büyük harften oluşmalıdır (örn: USD).");
    }
}

