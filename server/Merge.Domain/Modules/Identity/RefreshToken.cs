using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// RefreshToken Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class RefreshToken : BaseEntity, IAggregateRoot
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

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

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
        Guard.AgainstLength(tokenHash, 256, nameof(tokenHash));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Refresh token expiration date must be in the future");
        
        if (!string.IsNullOrEmpty(createdByIp))
        {
            Guard.AgainstLength(createdByIp, 50, nameof(createdByIp));
        }

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

        // ✅ BOLUM 1.5: Domain Events - RefreshTokenCreatedEvent
        refreshToken.AddDomainEvent(new RefreshTokenCreatedEvent(refreshToken.Id, userId, expiresAt));

        return refreshToken;
    }

    // ✅ BOLUM 1.1: Domain Method - Revoke token
    public void Revoke(string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new DomainException("Refresh token is already revoked");

        if (IsExpired)
            throw new DomainException("Refresh token is already expired");

        if (!string.IsNullOrEmpty(revokedByIp))
        {
            Guard.AgainstLength(revokedByIp, 50, nameof(revokedByIp));
        }
        
        if (!string.IsNullOrEmpty(replacedByTokenHash))
        {
            Guard.AgainstLength(replacedByTokenHash, 256, nameof(replacedByTokenHash));
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - RefreshTokenRevokedEvent
        AddDomainEvent(new RefreshTokenRevokedEvent(Id, UserId, revokedByIp, replacedByTokenHash));
    }
}
