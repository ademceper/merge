using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

public record GetActivePreOrderCampaignsQuery(
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderCampaignDto>>;

