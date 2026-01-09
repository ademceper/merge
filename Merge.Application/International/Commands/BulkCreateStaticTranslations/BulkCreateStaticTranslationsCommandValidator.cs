using FluentValidation;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class BulkCreateStaticTranslationsCommandValidator : AbstractValidator<BulkCreateStaticTranslationsCommand>
{
    public BulkCreateStaticTranslationsCommandValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Çeviriler zorunludur.")
            .Must(t => t.Count > 0).WithMessage("En az bir çeviri gereklidir.")
            .Must(t => t.Count <= 1000).WithMessage("Maksimum 1000 çeviri eklenebilir.");
    }
}

