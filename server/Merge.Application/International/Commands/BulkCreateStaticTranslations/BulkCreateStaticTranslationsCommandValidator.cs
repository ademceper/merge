using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

public class BulkCreateStaticTranslationsCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<BulkCreateStaticTranslationsCommand>
{
    public BulkCreateStaticTranslationsCommandValidator()
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

