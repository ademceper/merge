using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

public record GetPreOrderCampaignQuery(
    Guid CampaignId) : IRequest<PreOrderCampaignDto?>;

