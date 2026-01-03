using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// OrderVerification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderVerification : BaseEntity
{
    public Guid OrderId { get; set; }
    public string VerificationType { get; set; } = string.Empty; // Manual, Automatic, Phone, Email, Document
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public Guid? VerifiedByUserId { get; set; } // Admin/Staff who verified
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public string? VerificationMethod { get; set; } // Phone call, Email confirmation, ID check, etc.
    public bool RequiresManualReview { get; set; } = false;
    public int RiskScore { get; set; } = 0; // 0-100 risk score
    public string? RejectionReason { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public User? VerifiedBy { get; set; }
}

