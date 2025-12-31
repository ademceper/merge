namespace Merge.Application.DTOs.Analytics;

public class LiveChatStatsDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int WaitingSessions { get; set; }
    public int ResolvedSessions { get; set; }
    public decimal AverageResponseTime { get; set; } // in minutes
    public decimal AverageResolutionTime { get; set; } // in minutes
    public Dictionary<string, int> SessionsByDepartment { get; set; } = new();
    public Dictionary<string, int> SessionsByAgent { get; set; } = new();
}
