using Merge.Application.DTOs.Security;
namespace Merge.Application.Interfaces.Security;

public interface IAccountSecurityMonitoringService
{
    Task<AccountSecurityEventDto> LogSecurityEventAsync(CreateAccountSecurityEventDto dto);
    Task<IEnumerable<AccountSecurityEventDto>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, int page = 1, int pageSize = 20);
    Task<IEnumerable<AccountSecurityEventDto>> GetSuspiciousEventsAsync(int page = 1, int pageSize = 20);
    Task<bool> TakeActionAsync(Guid eventId, Guid actionTakenByUserId, string action, string? notes = null);
    Task<SecurityAlertDto> CreateSecurityAlertAsync(CreateSecurityAlertDto dto);
    Task<IEnumerable<SecurityAlertDto>> GetSecurityAlertsAsync(Guid? userId = null, string? severity = null, string? status = null, int page = 1, int pageSize = 20);
    Task<bool> AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedByUserId);
    Task<bool> ResolveAlertAsync(Guid alertId, Guid resolvedByUserId, string? resolutionNotes = null);
    Task<SecurityMonitoringSummaryDto> GetSecuritySummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
}

