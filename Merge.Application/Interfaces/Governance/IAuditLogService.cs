using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Interfaces.Governance;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
// ⚠️ OBSOLETE: Bu interface artık kullanılmamalı. MediatR Command/Query handler'ları kullanın.
[Obsolete("Use MediatR commands and queries instead. This service will be removed in a future version.")]
public interface IAuditLogService
{
    [Obsolete("Use CreateAuditLogCommand via MediatR instead")]
    Task LogAsync(CreateAuditLogDto auditDto, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    [Obsolete("Use GetAuditLogByIdQuery via MediatR instead")]
    Task<AuditLogDto?> GetAuditByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    [Obsolete("Use SearchAuditLogsQuery via MediatR instead")]
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
    [Obsolete("Use GetEntityHistoryQuery via MediatR instead")]
    Task<EntityAuditHistoryDto?> GetEntityHistoryAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    [Obsolete("Use GetAuditStatsQuery via MediatR instead")]
    Task<AuditStatsDto> GetAuditStatsAsync(int days = 30, CancellationToken cancellationToken = default);
    [Obsolete("Use GetUserAuditHistoryQuery via MediatR instead")]
    Task<IEnumerable<AuditLogDto>> GetUserAuditHistoryAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    [Obsolete("Use CompareChangesQuery via MediatR instead")]
    Task<IEnumerable<AuditComparisonDto>> CompareChangesAsync(Guid auditLogId, CancellationToken cancellationToken = default);
    [Obsolete("Use DeleteOldAuditLogsCommand via MediatR instead")]
    Task DeleteOldAuditLogsAsync(int daysToKeep = 365, CancellationToken cancellationToken = default);
}