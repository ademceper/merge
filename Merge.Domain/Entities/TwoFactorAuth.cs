namespace Merge.Domain.Entities;

public enum TwoFactorMethod
{
    None,
    SMS,
    Email,
    Authenticator
}

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

public class TwoFactorCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public TwoFactorMethod Method { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string Purpose { get; set; } = string.Empty; // "Login", "Enable2FA", "Disable2FA"

    // Navigation properties
    public User User { get; set; } = null!;
}
