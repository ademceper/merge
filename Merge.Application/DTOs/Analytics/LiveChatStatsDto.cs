namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
// ⚠️ NOT: Dictionary kullanımı .cursorrules'a göre yasak, ancak mevcut yapıyı koruyoruz
public record LiveChatStatsDto(
    int TotalSessions,
    int ActiveSessions,
    int WaitingSessions,
    int ResolvedSessions,
    decimal AverageResponseTime, // in minutes
    decimal AverageResolutionTime, // in minutes
    Dictionary<string, int> SessionsByDepartment,
    Dictionary<string, int> SessionsByAgent
);
