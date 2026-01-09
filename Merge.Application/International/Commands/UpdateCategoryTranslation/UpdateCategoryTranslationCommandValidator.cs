using FluentValidation;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UpdateCategoryTranslationCommandValidator : AbstractValidator<UpdateCategoryTranslationCommand>
{
    public UpdateCategoryTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(200).WithMessage("Kategori adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");
    }
}

