using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetSecuritySummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSecuritySummaryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SecurityMonitoringSummaryDto>;
