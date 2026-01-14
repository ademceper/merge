using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTopProductsQuery(
    DateTime StartDate,
    DateTime EndDate,
    int Limit
) : IRequest<List<TopProductDto>>;

