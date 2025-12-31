namespace Merge.Application.DTOs.Analytics;

public class SystemHealthDto
{
    public string DatabaseStatus { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public DateTime LastBackup { get; set; }
    public string DiskUsage { get; set; } = string.Empty;
    public string MemoryUsage { get; set; } = string.Empty;
    public int ActiveSessions { get; set; }
}
