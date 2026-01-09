using MediatR;

namespace Merge.Application.Governance.Queries.GetAcceptanceStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAcceptanceStatsQuery() : IRequest<Dictionary<string, int>>;

