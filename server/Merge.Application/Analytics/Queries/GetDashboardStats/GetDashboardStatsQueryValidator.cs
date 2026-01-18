using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetDashboardStats;

public class GetDashboardStatsQueryValidator : AbstractValidator<GetDashboardStatsQuery>
{
    public GetDashboardStatsQueryValidator()
    {
        // GetDashboardStatsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

