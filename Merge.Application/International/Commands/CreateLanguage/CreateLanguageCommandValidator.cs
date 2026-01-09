using FluentValidation;

namespace Merge.Application.International.Commands.CreateLanguage;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateLanguageCommandValidator : AbstractValidator<CreateLanguageCommand>
{
    public CreateLanguageCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");

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

