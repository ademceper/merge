using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UpdateStaticTranslationCommandValidator : AbstractValidator<UpdateStaticTranslationCommand>
{
    public UpdateStaticTranslationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Çeviri ID'si zorunludur.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(5000).WithMessage("Değer en fazla 5000 karakter olabilir.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.");
    }
}

