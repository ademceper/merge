using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

public class ClearRecentlyViewedCommandValidator : AbstractValidator<ClearRecentlyViewedCommand>
{
    public ClearRecentlyViewedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

