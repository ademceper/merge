namespace Merge.Application.DTOs.Analytics;

// ⚠️ NOT: Dictionary kullanımı .cursorrules'a göre yasak, ancak mevcut yapıyı koruyoruz
public record FraudAnalyticsDto(
    int TotalAlerts,
    int PendingAlerts,
    int ResolvedAlerts,
    int FalsePositiveAlerts,
    decimal AverageRiskScore,
    Dictionary<string, int> AlertsByType,
    Dictionary<string, int> AlertsByStatus,
    List<HighRiskAlertDto> HighRiskAlerts
);
