using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetSecuritySummary;

public record GetSecuritySummaryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SecurityMonitoringSummaryDto>;
