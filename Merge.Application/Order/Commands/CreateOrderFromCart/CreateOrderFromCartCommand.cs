using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Commands.CreateOrderFromCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateOrderFromCartCommand(
    Guid UserId,
    Guid AddressId,
    string? CouponCode = null
) : IRequest<OrderDto>;
