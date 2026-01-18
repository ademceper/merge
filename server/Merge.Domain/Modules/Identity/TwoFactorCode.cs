using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// TwoFactorCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string Status, Purpose YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TwoFactorCode : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    
    public TwoFactorMethod Method { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; } = false;
    public DateTime? UsedAt { get; private set; }
    
    public TwoFactorPurpose Purpose { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private TwoFactorCode() { }

    public static TwoFactorCode Create(
        Guid userId,
        string code,
        TwoFactorMethod method,
        DateTime expiresAt,
        TwoFactorPurpose purpose)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstLength(code, 20, nameof(code));
        
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

        twoFactorCode.AddDomainEvent(new TwoFactorCodeCreatedEvent(twoFactorCode.Id, userId, method, purpose));

        return twoFactorCode;
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Verification code is already used");

        if (IsExpired)
            throw new DomainException("Verification code has expired");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TwoFactorCodeUsedEvent(Id, UserId, Method, Purpose));
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
}

