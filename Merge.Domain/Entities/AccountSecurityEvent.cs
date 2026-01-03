namespace Merge.Domain.Entities;

/// <summary>
/// AccountSecurityEvent Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

