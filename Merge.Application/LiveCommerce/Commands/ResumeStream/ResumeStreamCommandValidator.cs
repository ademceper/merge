using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.ResumeStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ResumeStreamCommandValidator : AbstractValidator<ResumeStreamCommand>
{
    public ResumeStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
