using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.StartStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class StartStreamCommandValidator : AbstractValidator<StartStreamCommand>
{
    public StartStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}

