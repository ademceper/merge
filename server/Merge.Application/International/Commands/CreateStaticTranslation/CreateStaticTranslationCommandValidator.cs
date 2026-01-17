using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.CreateStaticTranslation;

public class CreateStaticTranslationCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<CreateStaticTranslationCommand>
{
    private readonly InternationalSettings config = settings.Value;

    public CreateStaticTranslationCommandValidator() : this(Options.Create(new InternationalSettings()))
    {
        

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Anahtar zorunludur.")
            .MaximumLength(config.MaxTranslationKeyLength).WithMessage($"Anahtar en fazla {config.MaxTranslationKeyLength} karakter olabilir.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(config.MaxTranslationValueLength).WithMessage($"Değer en fazla {config.MaxTranslationValueLength} karakter olabilir.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(config.MaxTranslationCategoryLength).WithMessage($"Kategori en fazla {config.MaxTranslationCategoryLength} karakter olabilir.");
    }
}

