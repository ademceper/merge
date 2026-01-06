using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAbandonedCartByIdQuery(Guid CartId) : IRequest<AbandonedCartDto?>;

