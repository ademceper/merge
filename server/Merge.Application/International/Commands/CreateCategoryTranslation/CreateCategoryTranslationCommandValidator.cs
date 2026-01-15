using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

public class CreateCategoryTranslationCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<CreateCategoryTranslationCommand>
{
    public CreateCategoryTranslationCommandValidator()
    {
        var config = settings.Value;

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(config.MaxCategoryTranslationNameLength).WithMessage($"Kategori adı en fazla {config.MaxCategoryTranslationNameLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(config.MaxCategoryTranslationDescriptionLength).WithMessage($"Açıklama en fazla {config.MaxCategoryTranslationDescriptionLength} karakter olabilir.");
    }
}

