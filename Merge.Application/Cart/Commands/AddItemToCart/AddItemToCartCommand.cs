using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.AddItemToCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddItemToCartCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity
) : IRequest<CartItemDto>;

