using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

public record DeactivatePreOrderCampaignCommand(
    Guid CampaignId) : IRequest<bool>;

