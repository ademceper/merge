using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Governance.Queries.GetAuditStats;

public record GetAuditStatsQuery(
    int Days = 30
) : IRequest<AuditStatsDto>;

