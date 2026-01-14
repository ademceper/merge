using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetBestSellers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetBestSellersQuery(
    int Limit
) : IRequest<List<TopProductDto>>;

