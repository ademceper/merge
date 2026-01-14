using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class BulkCreateStaticTranslationsCommandValidator : AbstractValidator<BulkCreateStaticTranslationsCommand>
{
    public BulkCreateStaticTranslationsCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Çeviriler zorunludur.")
            .Must(t => t.Count > 0).WithMessage("En az bir çeviri gereklidir.")
            .Must(t => t.Count <= 1000).WithMessage("Maksimum 1000 çeviri eklenebilir.");
    }
}

