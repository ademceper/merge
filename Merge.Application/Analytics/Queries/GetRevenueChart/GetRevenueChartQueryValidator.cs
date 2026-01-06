using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetRevenueChart;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetRevenueChartQueryValidator : AbstractValidator<GetRevenueChartQuery>
{
    public GetRevenueChartQueryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThanOrEqualTo(1).WithMessage("Gün sayısı 1 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(365).WithMessage("Gün sayısı 365'den küçük veya eşit olmalıdır");
    }
}

