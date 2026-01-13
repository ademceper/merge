using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CancelStreamCommandValidator : AbstractValidator<CancelStreamCommand>
{
    public CancelStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
