using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetPersonalizedRecommendations;

public record GetPersonalizedRecommendationsQuery(
    Guid UserId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
