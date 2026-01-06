using FluentValidation;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

public class GetPreOrderCampaignQueryValidator : AbstractValidator<GetPreOrderCampaignQuery>
{
    public GetPreOrderCampaignQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Kampanya ID zorunludur.");
    }
}

