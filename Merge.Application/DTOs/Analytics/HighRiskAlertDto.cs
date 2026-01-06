namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record HighRiskAlertDto(
    Guid AlertId,
    string AlertType,
    int RiskScore,
    string Status,
    DateTime CreatedAt
);
