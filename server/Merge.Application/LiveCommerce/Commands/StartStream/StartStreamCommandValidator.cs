using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.StartStream;

public class StartStreamCommandValidator : AbstractValidator<StartStreamCommand>
{
    public StartStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
