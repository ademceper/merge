using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Governance;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface IAuditLogService
{
    Task LogAsync(CreateAuditLogDto auditDto, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<AuditLogDto?> GetAuditByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
    Task<EntityAuditHistoryDto?> GetEntityHistoryAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<AuditStatsDto> GetAuditStatsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLogDto>> GetUserAuditHistoryAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditComparisonDto>> CompareChangesAsync(Guid auditLogId, CancellationToken cancellationToken = default);
    Task DeleteOldAuditLogsAsync(int daysToKeep = 365, CancellationToken cancellationToken = default);
}
