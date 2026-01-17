using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

public class UpdateStaticTranslationCommandValidator(IOptions<InternationalSettings> settings) : AbstractValidator<UpdateStaticTranslationCommand>
{
    private readonly InternationalSettings config = settings.Value;

    public UpdateStaticTranslationCommandValidator() : this(Options.Create(new InternationalSettings()))
    {

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

