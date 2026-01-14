using FluentValidation;

namespace Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdatePickPackItemStatusCommandValidator : AbstractValidator<UpdatePickPackItemStatusCommand>
{
    public UpdatePickPackItemStatusCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Pick-pack kalemi ID'si zorunludur.");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Konum en fazla 200 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

