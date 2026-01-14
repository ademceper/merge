using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

public class GetPreOrderCampaignQueryValidator : AbstractValidator<GetPreOrderCampaignQuery>
{
    public GetPreOrderCampaignQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Kampanya ID zorunludur.");
    }
}

