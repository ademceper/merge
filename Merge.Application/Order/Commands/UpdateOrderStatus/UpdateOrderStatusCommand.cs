using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.UpdateOrderStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status
) : IRequest<OrderDto>;
