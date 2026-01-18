using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Analytics.Queries.GetRecentOrders;

public record GetRecentOrdersQuery(
    int Count
) : IRequest<IEnumerable<OrderDto>>;

