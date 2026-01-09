using FluentValidation;

namespace Merge.Application.International.Commands.CreateProductTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateProductTranslationCommandValidator : AbstractValidator<CreateProductTranslationCommand>
{
    public CreateProductTranslationCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Açıklama en fazla 5000 karakter olabilir.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Kısa açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200).WithMessage("Meta başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).WithMessage("Meta açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200).WithMessage("Meta anahtar kelimeler en fazla 200 karakter olabilir.");
    }
}

