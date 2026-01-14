using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCampaignAnalyticsQueryValidator : AbstractValidator<GetCampaignAnalyticsQuery>
{
    public GetCampaignAnalyticsQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Campaign ID zorunludur.");
    }
}
