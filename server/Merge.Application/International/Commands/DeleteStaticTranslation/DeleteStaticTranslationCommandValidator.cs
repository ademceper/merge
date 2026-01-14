using FluentValidation;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class DeleteStaticTranslationCommandValidator : AbstractValidator<DeleteStaticTranslationCommand>
{
    public DeleteStaticTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");
    }
}

