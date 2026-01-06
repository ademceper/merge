using FluentValidation;

namespace Merge.Application.Cart.Commands.TrackEmailOpen;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class TrackEmailOpenCommandValidator : AbstractValidator<TrackEmailOpenCommand>
{
    public TrackEmailOpenCommandValidator()
    {
        RuleFor(x => x.EmailId)
            .NotEmpty().WithMessage("Email ID zorunludur");
    }
}

