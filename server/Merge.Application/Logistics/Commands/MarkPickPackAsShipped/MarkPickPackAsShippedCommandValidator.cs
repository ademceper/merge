using FluentValidation;

namespace Merge.Application.Logistics.Commands.MarkPickPackAsShipped;

public class MarkPickPackAsShippedCommandValidator : AbstractValidator<MarkPickPackAsShippedCommand>
{
    public MarkPickPackAsShippedCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");
    }
}

