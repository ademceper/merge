using FluentValidation;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Cart.Commands.TrackEmailClick;

public class TrackEmailClickCommandValidator : AbstractValidator<TrackEmailClickCommand>
{
    public TrackEmailClickCommandValidator()
    {
        RuleFor(x => x.EmailId)
            .NotEmpty().WithMessage("Email ID zorunludur");
    }
}

