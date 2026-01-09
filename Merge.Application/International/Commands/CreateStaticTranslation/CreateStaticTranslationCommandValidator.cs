using FluentValidation;

namespace Merge.Application.International.Commands.CreateStaticTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateStaticTranslationCommandValidator : AbstractValidator<CreateStaticTranslationCommand>
{
    public CreateStaticTranslationCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Anahtar zorunludur.")
            .MaximumLength(200).WithMessage("Anahtar en fazla 200 karakter olabilir.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(5000).WithMessage("Değer en fazla 5000 karakter olabilir.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.");
    }
}

