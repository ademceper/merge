using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetUserPreOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserPreOrdersQuery(
    Guid UserId,
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderDto>>;

