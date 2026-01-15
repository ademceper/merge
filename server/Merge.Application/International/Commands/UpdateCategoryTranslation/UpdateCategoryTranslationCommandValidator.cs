using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

public class UpdateCategoryTranslationCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<UpdateCategoryTranslationCommand>
{
    public UpdateCategoryTranslationCommandValidator()
    {
        var config = settings.Value;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(config.MaxCategoryTranslationNameLength).WithMessage($"Kategori adı en fazla {config.MaxCategoryTranslationNameLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(config.MaxCategoryTranslationDescriptionLength).WithMessage($"Açıklama en fazla {config.MaxCategoryTranslationDescriptionLength} karakter olabilir.");
    }
}

