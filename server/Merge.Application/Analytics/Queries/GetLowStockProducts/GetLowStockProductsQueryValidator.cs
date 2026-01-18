using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetLowStockProducts;

public class GetLowStockProductsQueryValidator : AbstractValidator<GetLowStockProductsQuery>
{
    public GetLowStockProductsQueryValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0).WithMessage("Eşik değeri 0 veya daha büyük olmalıdır");
    }
}

