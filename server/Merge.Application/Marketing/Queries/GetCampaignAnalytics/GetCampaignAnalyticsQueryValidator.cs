using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

public class GetCampaignAnalyticsQueryValidator : AbstractValidator<GetCampaignAnalyticsQuery>
{
    public GetCampaignAnalyticsQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Campaign ID zorunludur.");
    }
}
