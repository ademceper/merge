using FluentValidation;

namespace Merge.Application.Cart.Commands.TrackEmailClick;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class TrackEmailClickCommandValidator : AbstractValidator<TrackEmailClickCommand>
{
    public TrackEmailClickCommandValidator()
    {
        RuleFor(x => x.EmailId)
            .NotEmpty().WithMessage("Email ID zorunludur");
    }
}

