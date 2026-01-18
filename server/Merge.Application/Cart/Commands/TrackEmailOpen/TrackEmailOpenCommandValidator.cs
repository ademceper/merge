using FluentValidation;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Cart.Commands.TrackEmailOpen;

public class TrackEmailOpenCommandValidator : AbstractValidator<TrackEmailOpenCommand>
{
    public TrackEmailOpenCommandValidator()
    {
        RuleFor(x => x.EmailId)
            .NotEmpty().WithMessage("Email ID zorunludur");
    }
}

