using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetPersonalizedRecommendations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPersonalizedRecommendationsQuery(
    Guid UserId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
