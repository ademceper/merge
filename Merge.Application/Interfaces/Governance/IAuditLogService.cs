using Merge.Application.DTOs.Security;

namespace Merge.Application.Interfaces.Governance;

public interface IAuditLogService
{
    Task LogAsync(CreateAuditLogDto auditDto, string ipAddress, string userAgent);
    Task<AuditLogDto?> GetAuditByIdAsync(Guid id);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter);
    Task<EntityAuditHistoryDto?> GetEntityHistoryAsync(string entityType, Guid entityId);
    Task<AuditStatsDto> GetAuditStatsAsync(int days = 30);
    Task<IEnumerable<AuditLogDto>> GetUserAuditHistoryAsync(Guid userId, int days = 30);
    Task<IEnumerable<AuditComparisonDto>> CompareChangesAsync(Guid auditLogId);
    Task DeleteOldAuditLogsAsync(int daysToKeep = 365);
}
