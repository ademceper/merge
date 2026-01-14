using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class UpdateStaticTranslationCommandValidator : AbstractValidator<UpdateStaticTranslationCommand>
{
    public UpdateStaticTranslationCommandValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

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

