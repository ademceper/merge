using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyStats;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetLoyaltyStatsQueryValidator() : AbstractValidator<GetLoyaltyStatsQuery>
{
    public GetLoyaltyStatsQueryValidator()
    {
        // No validation needed for empty query
    }
}
