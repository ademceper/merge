using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ClearRecentlyViewedCommandValidator : AbstractValidator<ClearRecentlyViewedCommand>
{
    public ClearRecentlyViewedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

