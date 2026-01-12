using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// TwoFactorCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string Status YASAK)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TwoFactorCode : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public TwoFactorMethod Method { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; } = false;
    public DateTime? UsedAt { get; private set; }
    public string Purpose { get; private set; } = string.Empty; // "Login", "Enable2FA", "Disable2FA"

    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TwoFactorCode() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static TwoFactorCode Create(
        Guid userId,
        string code,
        TwoFactorMethod method,
        DateTime expiresAt,
        string purpose)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNullOrEmpty(purpose, nameof(purpose));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Verification code expiration date must be in the future");

        var twoFactorCode = new TwoFactorCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Method = method,
            ExpiresAt = expiresAt,
            Purpose = purpose,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        return twoFactorCode;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as used
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Verification code is already used");

        if (IsExpired)
            throw new DomainException("Verification code has expired");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
}

