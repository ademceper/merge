using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.SplitOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SplitOrderCommand(
    Guid OrderId,
    CreateOrderSplitDto Dto
) : IRequest<OrderSplitDto>;
