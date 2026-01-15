using FluentValidation;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

public class DeleteCategoryTranslationCommandValidator : AbstractValidator<DeleteCategoryTranslationCommand>
{
    public DeleteCategoryTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");
    }
}

