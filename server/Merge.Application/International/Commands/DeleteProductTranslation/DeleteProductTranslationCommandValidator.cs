using FluentValidation;

namespace Merge.Application.International.Commands.DeleteProductTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeleteProductTranslationCommandValidator : AbstractValidator<DeleteProductTranslationCommand>
{
    public DeleteProductTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");
    }
}

