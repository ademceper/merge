using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Seller.Queries.GetSellerOrders;

public record GetSellerOrdersQuery(
    Guid SellerId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderDto>>;
