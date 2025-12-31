namespace Merge.Application.DTOs.Analytics;

public class HighRiskAlertDto
{
    public Guid AlertId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
