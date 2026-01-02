using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

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

public class AccountSecurityEvent : BaseEntity
{
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty; // Login, PasswordChange, EmailChange, SuspiciousActivity, FailedLogin, etc.
    public string Severity { get; set; } = "Info"; // Info, Warning, Critical
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; } // Country/City
    public string? DeviceFingerprint { get; set; }
    public bool IsSuspicious { get; set; } = false;
    public string? Details { get; set; } // JSON for additional details
    public bool RequiresAction { get; set; } = false;
    public string? ActionTaken { get; set; } // Account locked, Password reset required, etc.
    public Guid? ActionTakenByUserId { get; set; } // Admin who took action
    public DateTime? ActionTakenAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? ActionTakenBy { get; set; }
}

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

