using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Analytics.Queries.GetRecentOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRecentOrdersQuery(
    int Count
) : IRequest<IEnumerable<OrderDto>>;

