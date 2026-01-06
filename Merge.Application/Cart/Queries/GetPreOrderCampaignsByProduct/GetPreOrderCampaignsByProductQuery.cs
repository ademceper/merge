using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;

public record GetPreOrderCampaignsByProductQuery(
    Guid ProductId,
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderCampaignDto>>;

