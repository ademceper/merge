using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.SearchAuditLogs;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SearchAuditLogsQuery(
    Guid? UserId = null,
    string? UserEmail = null,
    string? Action = null,
    string? EntityType = null,
    Guid? EntityId = null,
    string? TableName = null,
    string? Severity = null,
    string? Module = null,
    bool? IsSuccessful = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? IpAddress = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<PagedResult<AuditLogDto>>;
