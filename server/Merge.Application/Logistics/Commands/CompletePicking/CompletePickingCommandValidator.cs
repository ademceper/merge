using FluentValidation;

namespace Merge.Application.Logistics.Commands.CompletePicking;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CompletePickingCommandValidator : AbstractValidator<CompletePickingCommand>
{
    public CompletePickingCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");
    }
}

