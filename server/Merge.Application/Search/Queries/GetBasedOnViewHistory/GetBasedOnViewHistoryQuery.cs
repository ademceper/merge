using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetBasedOnViewHistory;

public record GetBasedOnViewHistoryQuery(
    Guid UserId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
