using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Cart.Queries.GetRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRecentlyViewedQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;

