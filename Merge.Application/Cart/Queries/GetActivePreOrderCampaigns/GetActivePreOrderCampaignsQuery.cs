using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetActivePreOrderCampaignsQuery(
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderCampaignDto>>;

