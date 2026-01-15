using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.ResumeStream;

public class ResumeStreamCommandValidator : AbstractValidator<ResumeStreamCommand>
{
    public ResumeStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
