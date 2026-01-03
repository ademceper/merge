using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// FraudAlert Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FraudAlert : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public string AlertType { get; set; } = string.Empty; // Order, Payment, Account, Behavior
    public int RiskScore { get; set; } = 0; // Calculated risk score (0-100)
    public FraudAlertStatus Status { get; set; } = FraudAlertStatus.Pending;
    public string? Reason { get; set; } // Why this alert was triggered
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public string? MatchedRules { get; set; } // JSON array of matched rule IDs
}

