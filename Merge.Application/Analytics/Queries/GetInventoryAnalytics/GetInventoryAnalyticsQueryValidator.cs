using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetInventoryAnalytics;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetInventoryAnalyticsQueryValidator : AbstractValidator<GetInventoryAnalyticsQuery>
{
    public GetInventoryAnalyticsQueryValidator()
    {
        // GetInventoryAnalyticsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

