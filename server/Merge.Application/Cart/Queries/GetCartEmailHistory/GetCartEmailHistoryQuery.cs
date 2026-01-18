using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartEmailHistory;

public record GetCartEmailHistoryQuery(
    Guid CartId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AbandonedCartEmailDto>>;

