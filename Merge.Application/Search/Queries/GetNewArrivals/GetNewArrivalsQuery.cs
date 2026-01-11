using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Search.Queries.GetNewArrivals;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetNewArrivalsQuery(
    int Days = 30,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
