using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWorstPerformersQuery(
    int Limit
) : IRequest<List<TopProductDto>>;

