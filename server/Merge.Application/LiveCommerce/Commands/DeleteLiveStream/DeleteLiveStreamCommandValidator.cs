using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteLiveStreamCommandValidator : AbstractValidator<DeleteLiveStreamCommand>
{
    public DeleteLiveStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}

