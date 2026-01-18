using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetTrendingProducts;

public record GetTrendingProductsQuery(
    int Days = 7,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
