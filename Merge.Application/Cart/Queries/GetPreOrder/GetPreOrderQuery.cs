using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPreOrderQuery(
    Guid PreOrderId) : IRequest<PreOrderDto?>;

