using FluentValidation;

namespace Merge.Application.Logistics.Commands.MarkPickPackAsShipped;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class MarkPickPackAsShippedCommandValidator : AbstractValidator<MarkPickPackAsShippedCommand>
{
    public MarkPickPackAsShippedCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");
    }
}

