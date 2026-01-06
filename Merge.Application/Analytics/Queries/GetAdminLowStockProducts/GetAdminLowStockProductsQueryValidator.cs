using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAdminLowStockProductsQueryValidator : AbstractValidator<GetAdminLowStockProductsQuery>
{
    public GetAdminLowStockProductsQueryValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0).WithMessage("Eşik değeri 0 veya daha büyük olmalıdır");
    }
}

