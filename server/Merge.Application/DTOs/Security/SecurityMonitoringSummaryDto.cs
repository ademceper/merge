namespace Merge.Application.DTOs.Security;

public class SecurityMonitoringSummaryDto
{
    public int TotalSecurityEvents { get; set; }
    public int SuspiciousEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int PendingAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    public List<SecurityAlertDto> RecentCriticalAlerts { get; set; } = new();
}
