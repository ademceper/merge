using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetAbandonedCarts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAbandonedCartsQueryValidator : AbstractValidator<GetAbandonedCartsQuery>
{
    public GetAbandonedCartsQueryValidator()
    {
        RuleFor(x => x.MinHours)
            .GreaterThan(0).WithMessage("Minimum saat 0'dan büyük olmalıdır");

        RuleFor(x => x.MaxDays)
            .GreaterThan(0).WithMessage("Maksimum gün 0'dan büyük olmalıdır");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır");
    }
}

