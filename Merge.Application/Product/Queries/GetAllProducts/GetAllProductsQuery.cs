using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProducts;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
