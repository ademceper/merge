using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Search.Queries.GetTrendingProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTrendingProductsQuery(
    int Days = 7,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
