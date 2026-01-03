using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// PaymentFraudPrevention Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PaymentFraudPrevention : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string CheckType { get; set; } = string.Empty; // CVV, 3DS, Address, Velocity, Device
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public bool IsBlocked { get; set; } = false;
    public string? BlockReason { get; set; }
    public int RiskScore { get; set; } = 0; // 0-100
    public string? CheckResult { get; set; } // JSON for detailed results
    public DateTime? CheckedAt { get; set; }
    public string? DeviceFingerprint { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public Payment Payment { get; set; } = null!;
}

