using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetCartByCartItemId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCartByCartItemIdQuery(Guid CartItemId) : IRequest<CartDto?>;

