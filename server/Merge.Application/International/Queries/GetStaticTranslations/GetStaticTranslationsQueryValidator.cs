using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetStaticTranslations;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetStaticTranslationsQueryValidator : AbstractValidator<GetStaticTranslationsQuery>
{
    public GetStaticTranslationsQueryValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Category)
            .MaximumLength(config.MaxTranslationCategoryLength).WithMessage($"Kategori en fazla {config.MaxTranslationCategoryLength} karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

