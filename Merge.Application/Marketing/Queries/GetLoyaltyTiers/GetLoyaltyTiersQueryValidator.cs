using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTiers;

public class GetLoyaltyTiersQueryValidator : AbstractValidator<GetLoyaltyTiersQuery>
{
    public GetLoyaltyTiersQueryValidator()
    {
        // No validation needed for empty query
    }
}
