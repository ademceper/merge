using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

public record GetActivePreOrderCampaignsQuery(
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderCampaignDto>>;

