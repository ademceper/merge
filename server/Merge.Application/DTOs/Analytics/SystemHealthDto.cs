namespace Merge.Application.DTOs.Analytics;

public record SystemHealthDto(
    string DatabaseStatus,
    int TotalRecords,
    DateTime LastBackup,
    string DiskUsage,
    string MemoryUsage,
    int ActiveSessions
);
