using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetBasedOnViewHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetBasedOnViewHistoryQuery(
    Guid UserId,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
