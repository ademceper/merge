using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

public class UpdateStaticTranslationCommandValidator : AbstractValidator<UpdateStaticTranslationCommand>
{
    private readonly InternationalSettings config;

    public UpdateStaticTranslationCommandValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(config.MaxTranslationValueLength).WithMessage($"Değer en fazla {config.MaxTranslationValueLength} karakter olabilir.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(config.MaxTranslationCategoryLength).WithMessage($"Kategori en fazla {config.MaxTranslationCategoryLength} karakter olabilir.");
    }
}

