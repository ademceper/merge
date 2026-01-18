using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Security;

public interface IAccountSecurityMonitoringService
{
    Task<AccountSecurityEventDto> LogSecurityEventAsync(CreateAccountSecurityEventDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<AccountSecurityEventDto>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<AccountSecurityEventDto>> GetSuspiciousEventsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> TakeActionAsync(Guid eventId, Guid actionTakenByUserId, string action, string? notes = null, CancellationToken cancellationToken = default);
    Task<SecurityAlertDto> CreateSecurityAlertAsync(CreateSecurityAlertDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<SecurityAlertDto>> GetSecurityAlertsAsync(Guid? userId = null, string? severity = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedByUserId, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(Guid alertId, Guid resolvedByUserId, string? resolutionNotes = null, CancellationToken cancellationToken = default);
    Task<SecurityMonitoringSummaryDto> GetSecuritySummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

