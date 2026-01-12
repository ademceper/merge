using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCartByUserIdQuery(Guid UserId) : IRequest<CartDto>;

