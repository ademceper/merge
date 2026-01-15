using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.PauseStream;

public class PauseStreamCommandValidator : AbstractValidator<PauseStreamCommand>
{
    public PauseStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
