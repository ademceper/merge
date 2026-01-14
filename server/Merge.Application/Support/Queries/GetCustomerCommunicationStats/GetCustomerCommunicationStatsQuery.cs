using MediatR;

namespace Merge.Application.Support.Queries.GetCustomerCommunicationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCustomerCommunicationStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Dictionary<string, int>>;
