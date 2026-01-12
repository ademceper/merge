using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetSplitOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSplitOrdersQuery(
    Guid SplitOrderId
) : IRequest<IEnumerable<OrderSplitDto>>;
