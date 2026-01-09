using FluentValidation;

namespace Merge.Application.International.Queries.GetCategoryTranslations;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetCategoryTranslationsQueryValidator : AbstractValidator<GetCategoryTranslationsQuery>
{
    public GetCategoryTranslationsQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID'si zorunludur.");
    }
}

