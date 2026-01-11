using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Queries.GetOrderSplits;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetOrderSplitsQuery(
    Guid OrderId
) : IRequest<IEnumerable<OrderSplitDto>>;
