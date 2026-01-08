using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPreOrderCampaignQuery(
    Guid CampaignId) : IRequest<PreOrderCampaignDto?>;

