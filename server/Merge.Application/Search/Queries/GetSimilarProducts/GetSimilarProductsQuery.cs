using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetSimilarProducts;

public record GetSimilarProductsQuery(
    Guid ProductId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
