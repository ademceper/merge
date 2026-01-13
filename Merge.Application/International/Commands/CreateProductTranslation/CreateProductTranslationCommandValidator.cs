using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.CreateProductTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class CreateProductTranslationCommandValidator : AbstractValidator<CreateProductTranslationCommand>
{
    public CreateProductTranslationCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur.")
            .MaximumLength(config.MaxProductTranslationNameLength).WithMessage($"Ürün adı en fazla {config.MaxProductTranslationNameLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(config.MaxProductTranslationDescriptionLength).WithMessage($"Açıklama en fazla {config.MaxProductTranslationDescriptionLength} karakter olabilir.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(config.MaxProductTranslationShortDescriptionLength).WithMessage($"Kısa açıklama en fazla {config.MaxProductTranslationShortDescriptionLength} karakter olabilir.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(config.MaxProductTranslationMetaTitleLength).WithMessage($"Meta başlık en fazla {config.MaxProductTranslationMetaTitleLength} karakter olabilir.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(config.MaxProductTranslationMetaDescriptionLength).WithMessage($"Meta açıklama en fazla {config.MaxProductTranslationMetaDescriptionLength} karakter olabilir.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(config.MaxProductTranslationMetaKeywordsLength).WithMessage($"Meta anahtar kelimeler en fazla {config.MaxProductTranslationMetaKeywordsLength} karakter olabilir.");
    }
}

