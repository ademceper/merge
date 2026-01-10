using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetCampaignStatsQueryValidator : AbstractValidator<GetCampaignStatsQuery>
{
    // No validation needed for empty query
}
