using FluentValidation;

namespace Merge.Application.International.Commands.DeleteLanguage;

public class DeleteLanguageCommandValidator : AbstractValidator<DeleteLanguageCommand>
{
    public DeleteLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID zorunludur.");
    }
}

