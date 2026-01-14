using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTiers;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetLoyaltyTiersQueryValidator : AbstractValidator<GetLoyaltyTiersQuery>
{
    public GetLoyaltyTiersQueryValidator()
    {
        // No validation needed for empty query
    }
}
