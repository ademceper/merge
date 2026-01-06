namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record SystemHealthDto(
    string DatabaseStatus,
    int TotalRecords,
    DateTime LastBackup,
    string DiskUsage,
    string MemoryUsage,
    int ActiveSessions
);
