using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.PauseStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class PauseStreamCommandValidator : AbstractValidator<PauseStreamCommand>
{
    public PauseStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
