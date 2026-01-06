using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetDashboardStats;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetDashboardStatsQueryValidator : AbstractValidator<GetDashboardStatsQuery>
{
    public GetDashboardStatsQueryValidator()
    {
        // GetDashboardStatsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

