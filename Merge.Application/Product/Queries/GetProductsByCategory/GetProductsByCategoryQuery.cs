using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetProductsByCategory;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductsByCategoryQuery(
    Guid CategoryId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
