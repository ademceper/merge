using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Commands.Reorder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReorderCommand(
    Guid OrderId,
    Guid UserId
) : IRequest<OrderDto>;
