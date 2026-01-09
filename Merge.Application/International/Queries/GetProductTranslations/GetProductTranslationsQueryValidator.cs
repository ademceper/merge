using FluentValidation;

namespace Merge.Application.International.Queries.GetProductTranslations;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetProductTranslationsQueryValidator : AbstractValidator<GetProductTranslationsQuery>
{
    public GetProductTranslationsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");
    }
}

