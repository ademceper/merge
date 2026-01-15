using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

public class DeleteLiveStreamCommandValidator : AbstractValidator<DeleteLiveStreamCommand>
{
    public DeleteLiveStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
