using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// TwoFactorAuth Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TwoFactorAuth : BaseEntity
{
    public Guid UserId { get; set; }
    public TwoFactorMethod Method { get; set; }
    public string Secret { get; set; } = string.Empty; // For authenticator apps (TOTP secret)
    public string? PhoneNumber { get; set; } // For SMS
    public string? Email { get; set; } // For email
    public bool IsEnabled { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public string[]? BackupCodes { get; set; } // Backup codes for recovery
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? LockedUntil { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

