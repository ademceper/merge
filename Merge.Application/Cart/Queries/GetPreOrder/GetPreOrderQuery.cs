using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPreOrderQuery(
    Guid PreOrderId) : IRequest<PreOrderDto?>;

