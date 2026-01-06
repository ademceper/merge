using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.SearchProducts;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SearchProductsQuery(
    string SearchTerm,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
