using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// TwoFactorCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

