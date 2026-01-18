using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

public record GetPreOrderCampaignQuery(
    Guid CampaignId) : IRequest<PreOrderCampaignDto?>;

