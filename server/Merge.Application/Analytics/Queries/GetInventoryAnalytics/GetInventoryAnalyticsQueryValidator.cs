using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetInventoryAnalytics;

public class GetInventoryAnalyticsQueryValidator : AbstractValidator<GetInventoryAnalyticsQuery>
{
    public GetInventoryAnalyticsQueryValidator()
    {
        // GetInventoryAnalyticsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

