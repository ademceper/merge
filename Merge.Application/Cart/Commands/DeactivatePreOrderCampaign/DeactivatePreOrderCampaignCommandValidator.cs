using FluentValidation;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

public class DeactivatePreOrderCampaignCommandValidator : AbstractValidator<DeactivatePreOrderCampaignCommand>
{
    public DeactivatePreOrderCampaignCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Kampanya ID zorunludur.");
    }
}

