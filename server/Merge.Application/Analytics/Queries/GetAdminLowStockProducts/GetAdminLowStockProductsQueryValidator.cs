using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

public class GetAdminLowStockProductsQueryValidator : AbstractValidator<GetAdminLowStockProductsQuery>
{
    public GetAdminLowStockProductsQueryValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0).WithMessage("Eşik değeri 0 veya daha büyük olmalıdır");
    }
}

