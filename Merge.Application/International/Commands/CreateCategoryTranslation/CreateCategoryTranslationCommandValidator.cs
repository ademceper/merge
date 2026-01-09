using FluentValidation;

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateCategoryTranslationCommandValidator : AbstractValidator<CreateCategoryTranslationCommand>
{
    public CreateCategoryTranslationCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(200).WithMessage("Kategori adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");
    }
}

