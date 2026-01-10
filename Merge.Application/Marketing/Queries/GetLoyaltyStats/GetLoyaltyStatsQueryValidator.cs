using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyStats;

public class GetLoyaltyStatsQueryValidator : AbstractValidator<GetLoyaltyStatsQuery>
{
    public GetLoyaltyStatsQueryValidator()
    {
        // No validation needed for empty query
    }
}
