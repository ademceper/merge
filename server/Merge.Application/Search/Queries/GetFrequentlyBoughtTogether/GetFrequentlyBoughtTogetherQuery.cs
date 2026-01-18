using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;

public record GetFrequentlyBoughtTogetherQuery(
    Guid ProductId,
    int MaxResults = 5
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
