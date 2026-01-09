using FluentValidation;

namespace Merge.Application.International.Commands.DeleteLanguage;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteLanguageCommandValidator : AbstractValidator<DeleteLanguageCommand>
{
    public DeleteLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID zorunludur.");
    }
}

