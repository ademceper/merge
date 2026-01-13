using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCampaignStatsQueryValidator() : AbstractValidator<GetCampaignStatsQuery>
{
    // No validation needed for empty query
}
