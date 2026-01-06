using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetCartEmailHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCartEmailHistoryQuery(
    Guid CartId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AbandonedCartEmailDto>>;

