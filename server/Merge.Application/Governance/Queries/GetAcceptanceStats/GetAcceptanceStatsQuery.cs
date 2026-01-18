using MediatR;

namespace Merge.Application.Governance.Queries.GetAcceptanceStats;

public record GetAcceptanceStatsQuery() : IRequest<Dictionary<string, int>>;

