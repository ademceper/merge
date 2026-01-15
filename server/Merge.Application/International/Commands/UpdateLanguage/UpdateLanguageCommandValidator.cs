using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateLanguage;

public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    private readonly InternationalSettings config;

    public UpdateLanguageCommandValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;
        

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dil adı zorunludur.")
            .MaximumLength(config.MaxLanguageNameLength).WithMessage($"Dil adı en fazla {config.MaxLanguageNameLength} karakter olabilir.");

        RuleFor(x => x.NativeName)
            .NotEmpty().WithMessage("Yerel dil adı zorunludur.")
            .MaximumLength(config.MaxLanguageNativeNameLength).WithMessage($"Yerel dil adı en fazla {config.MaxLanguageNativeNameLength} karakter olabilir.");

        RuleFor(x => x.FlagIcon)
            .MaximumLength(config.MaxLanguageFlagIconLength).WithMessage($"Bayrak ikonu en fazla {config.MaxLanguageFlagIconLength} karakter olabilir.");
    }
}

