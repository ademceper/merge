using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.EndStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class EndStreamCommandValidator : AbstractValidator<EndStreamCommand>
{
    public EndStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}

