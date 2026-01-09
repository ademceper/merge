using FluentValidation;

namespace Merge.Application.International.Queries.GetProductTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetProductTranslationQueryValidator : AbstractValidator<GetProductTranslationQuery>
{
    public GetProductTranslationQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");
    }
}

