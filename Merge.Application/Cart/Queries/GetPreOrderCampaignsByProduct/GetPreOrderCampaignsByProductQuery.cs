using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPreOrderCampaignsByProductQuery(
    Guid ProductId,
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderCampaignDto>>;

