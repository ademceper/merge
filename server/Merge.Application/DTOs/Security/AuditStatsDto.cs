namespace Merge.Application.DTOs.Security;

public class AuditStatsDto
{
    public int TotalAudits { get; set; }
    public int TodayAudits { get; set; }
    public int FailedActions { get; set; }
    public int CriticalEvents { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public Dictionary<string, int> ActionsByModule { get; set; } = new();
    public Dictionary<string, int> ActionsBySeverity { get; set; } = new();
    public List<TopAuditUserDto> MostActiveUsers { get; set; } = new();
    public List<RecentCriticalEventDto> RecentCriticalEvents { get; set; } = new();
}
