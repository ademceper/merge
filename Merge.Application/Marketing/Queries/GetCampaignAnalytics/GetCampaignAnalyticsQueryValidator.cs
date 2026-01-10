using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetCampaignAnalyticsQueryValidator : AbstractValidator<GetCampaignAnalyticsQuery>
{
    public GetCampaignAnalyticsQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Campaign ID zorunludur.");
    }
}
