using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Seller.Queries.GetSellerOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetSellerOrdersQuery(
    Guid SellerId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderDto>>;
