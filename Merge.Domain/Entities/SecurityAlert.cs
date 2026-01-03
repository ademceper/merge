using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SecurityAlert Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SecurityAlert : BaseEntity
{
    public Guid? UserId { get; set; } // Null for system-wide alerts
    public string AlertType { get; set; } = string.Empty; // Account, Payment, Order, System
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public AlertStatus Status { get; set; } = AlertStatus.New;
    public Guid? AcknowledgedByUserId { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
    
    // Navigation properties
    public User? User { get; set; }
    public User? AcknowledgedBy { get; set; }
    public User? ResolvedBy { get; set; }
}

