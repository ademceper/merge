using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

public class CancelStreamCommandValidator : AbstractValidator<CancelStreamCommand>
{
    public CancelStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
