using FluentValidation;

namespace Merge.Application.International.Commands.UpdateLanguage;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    public UpdateLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dil adı zorunludur.")
            .MaximumLength(100).WithMessage("Dil adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.NativeName)
            .NotEmpty().WithMessage("Yerel dil adı zorunludur.")
            .MaximumLength(100).WithMessage("Yerel dil adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.FlagIcon)
            .MaximumLength(200).WithMessage("Bayrak ikonu en fazla 200 karakter olabilir.");
    }
}

