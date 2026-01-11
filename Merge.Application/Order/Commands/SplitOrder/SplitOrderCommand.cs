using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Commands.SplitOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SplitOrderCommand(
    Guid OrderId,
    CreateOrderSplitDto Dto
) : IRequest<OrderSplitDto>;
