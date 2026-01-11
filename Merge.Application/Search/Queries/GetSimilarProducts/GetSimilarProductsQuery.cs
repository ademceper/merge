using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSimilarProductsQuery(
    Guid ProductId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
