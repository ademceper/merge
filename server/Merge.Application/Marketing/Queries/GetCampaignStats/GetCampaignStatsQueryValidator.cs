using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

public class GetCampaignStatsQueryValidator : AbstractValidator<GetCampaignStatsQuery>
{
    // No validation needed for empty query
}
