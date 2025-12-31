namespace Merge.Application.DTOs.Analytics;

public class FraudAnalyticsDto
{
    public int TotalAlerts { get; set; }
    public int PendingAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int FalsePositiveAlerts { get; set; }
    public decimal AverageRiskScore { get; set; }
    public Dictionary<string, int> AlertsByType { get; set; } = new();
    public Dictionary<string, int> AlertsByStatus { get; set; } = new();
    public List<HighRiskAlertDto> HighRiskAlerts { get; set; } = new();
}
