using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Security.Queries.GetSecurityAlerts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSecurityAlertsQuery(
    Guid? UserId = null,
    string? Severity = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SecurityAlertDto>>;
