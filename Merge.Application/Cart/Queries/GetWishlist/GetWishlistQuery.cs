using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWishlistQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;

