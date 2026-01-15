using FluentValidation;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

public class DeleteStaticTranslationCommandValidator : AbstractValidator<DeleteStaticTranslationCommand>
{
    public DeleteStaticTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");
    }
}

