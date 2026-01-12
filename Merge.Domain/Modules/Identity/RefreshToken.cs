using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// RefreshToken Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class RefreshToken : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanmalı (PLAIN TEXT YASAK)
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    // Navigation property
    public virtual User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Domain Logic - Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private RefreshToken() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(tokenHash, nameof(tokenHash));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Refresh token expiration date must be in the future");

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        return refreshToken;
    }

    // ✅ BOLUM 1.1: Domain Method - Revoke token
    public void Revoke(string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new DomainException("Refresh token is already revoked");

        if (IsExpired)
            throw new DomainException("Refresh token is already expired");

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
