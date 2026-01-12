using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFrequentlyBoughtTogetherQuery(
    Guid ProductId,
    int MaxResults = 5
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
