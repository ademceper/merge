using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteSEOSettings;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteSEOSettingsCommandValidator : AbstractValidator<DeleteSEOSettingsCommand>
{
    public DeleteSEOSettingsCommandValidator()
    {
        RuleFor(x => x.PageType)
            .NotEmpty()
            .WithMessage("Sayfa tipi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Sayfa tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("Entity ID zorunludur.");
    }
}

