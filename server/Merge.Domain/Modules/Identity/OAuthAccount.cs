using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// OAuthAccount Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OAuthAccount : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = string.Empty; // google, facebook, apple
    public string ProviderUserId { get; private set; } = string.Empty; // External user ID from provider
    public string? Email { get; private set; }
    public string? Name { get; private set; }
    public string? PictureUrl { get; private set; }
    public string? AccessToken { get; private set; } // Encrypted
    public string? RefreshToken { get; private set; } // Encrypted
    public DateTime? TokenExpiresAt { get; private set; }
    public bool IsPrimary { get; private set; } = false; // Primary login method
    
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

    private OAuthAccount() { }

    public static OAuthAccount Create(
        Guid userId,
        string provider,
        string providerUserId,
        string? email = null,
        string? name = null,
        string? pictureUrl = null,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiresAt = null,
        bool isPrimary = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(provider, nameof(provider));
        Guard.AgainstLength(provider, 50, nameof(provider));
        Guard.AgainstNullOrEmpty(providerUserId, nameof(providerUserId));
        Guard.AgainstLength(providerUserId, 256, nameof(providerUserId));
        
        if (!string.IsNullOrEmpty(email))
        {
            var emailValueObject = new Email(email);
            email = emailValueObject.Value;
        }
        
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstLength(name, 200, nameof(name));
        }
        
        if (!string.IsNullOrEmpty(pictureUrl))
        {
            Guard.AgainstLength(pictureUrl, 500, nameof(pictureUrl));
            if (!Uri.TryCreate(pictureUrl, UriKind.Absolute, out _))
                throw new DomainException("Geçersiz picture URL formatı");
        }

        var oauthAccount = new OAuthAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = tokenExpiresAt,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };

        oauthAccount.AddDomainEvent(new OAuthAccountCreatedEvent(oauthAccount.Id, userId, provider, providerUserId, isPrimary));

        return oauthAccount;
    }

    public void UpdateTokens(string? accessToken, string? refreshToken, DateTime? tokenExpiresAt)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            Guard.AgainstLength(accessToken, 2000, nameof(accessToken)); // Encrypted token can be long
        }
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            Guard.AgainstLength(refreshToken, 2000, nameof(refreshToken)); // Encrypted token can be long
        }
        
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OAuthAccountUpdatedEvent(Id, UserId, Provider));
    }

    public void SetAsPrimary()
    {
        if (IsPrimary)
            return;

        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OAuthAccountSetAsPrimaryEvent(Id, UserId, Provider));
    }

    public void RemovePrimaryStatus()
    {
        if (!IsPrimary)
            return;

        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OAuthAccountRemovedPrimaryStatusEvent(Id, UserId, Provider));
    }

    public bool IsTokenExpired => TokenExpiresAt.HasValue && DateTime.UtcNow >= TokenExpiresAt.Value;
}

