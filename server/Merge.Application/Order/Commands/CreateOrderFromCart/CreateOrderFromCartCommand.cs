using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CreateOrderFromCart;

public record CreateOrderFromCartCommand(
    Guid UserId,
    Guid AddressId,
    string? CouponCode = null
) : IRequest<OrderDto>;
