using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetBestSellers;

public record GetBestSellersQuery(
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
