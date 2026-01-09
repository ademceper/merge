using FluentValidation;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeleteCategoryTranslationCommandValidator : AbstractValidator<DeleteCategoryTranslationCommand>
{
    public DeleteCategoryTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");
    }
}

