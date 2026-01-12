using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Queries.GetStaticTranslations;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetStaticTranslationsQueryValidator : AbstractValidator<GetStaticTranslationsQuery>
{
    public GetStaticTranslationsQueryValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

