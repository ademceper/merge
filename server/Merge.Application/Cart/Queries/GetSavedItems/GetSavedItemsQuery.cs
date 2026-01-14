using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetSavedItems;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSavedItemsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SavedCartItemDto>>;

