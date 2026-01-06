using MediatR;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

public record DeactivatePreOrderCampaignCommand(
    Guid CampaignId) : IRequest<bool>;

