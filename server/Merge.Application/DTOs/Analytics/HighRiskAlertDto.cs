namespace Merge.Application.DTOs.Analytics;

public record HighRiskAlertDto(
    Guid AlertId,
    string AlertType,
    int RiskScore,
    string Status,
    DateTime CreatedAt
);
