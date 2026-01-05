using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Orders.Commands.CreateOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateOrderCommand(
    Guid UserId,
    Guid? AddressId,
    string? CouponCode,
    string? Notes
) : IRequest<OrderDto>;

