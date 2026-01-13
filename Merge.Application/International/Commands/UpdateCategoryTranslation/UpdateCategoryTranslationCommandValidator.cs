using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class UpdateCategoryTranslationCommandValidator : AbstractValidator<UpdateCategoryTranslationCommand>
{
    public UpdateCategoryTranslationCommandValidator(IOptions<InternationalSettings> settings)
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

