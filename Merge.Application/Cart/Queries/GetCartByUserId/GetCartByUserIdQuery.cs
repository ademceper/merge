using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCartByUserIdQuery(Guid UserId) : IRequest<CartDto>;

