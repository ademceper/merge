using FluentValidation;

namespace Merge.Application.International.Commands.UpdateProductTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UpdateProductTranslationCommandValidator : AbstractValidator<UpdateProductTranslationCommand>
{
    public UpdateProductTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");

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

