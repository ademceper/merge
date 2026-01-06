using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Queries.GetOrderById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetOrderByIdQuery(
    Guid OrderId
) : IRequest<OrderDto?>;
