using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Governance.Queries.GetAuditStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAuditStatsQuery(
    int Days = 30
) : IRequest<AuditStatsDto>;

