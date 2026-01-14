using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// OAuthProvider Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OAuthProvider : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty; // Google, Facebook, Apple
    public string ProviderKey { get; private set; } = string.Empty; // google, facebook, apple
    public string ClientId { get; private set; } = string.Empty;
    public string ClientSecret { get; private set; } = string.Empty; // Encrypted
    public string? RedirectUri { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Scopes { get; private set; } // Comma separated scopes
    public string? Settings { get; private set; } // JSON for provider-specific settings

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
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
    private OAuthProvider() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static OAuthProvider Create(
        string name,
        string providerKey,
        string clientId,
        string clientSecret,
        string? redirectUri = null,
        bool isActive = true,
        string? scopes = null,
        string? settings = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 100, nameof(name));
        Guard.AgainstNullOrEmpty(providerKey, nameof(providerKey));
        Guard.AgainstLength(providerKey, 50, nameof(providerKey));
        Guard.AgainstNullOrEmpty(clientId, nameof(clientId));
        Guard.AgainstLength(clientId, 256, nameof(clientId));
        Guard.AgainstNullOrEmpty(clientSecret, nameof(clientSecret));
        Guard.AgainstLength(clientSecret, 512, nameof(clientSecret));

        if (!string.IsNullOrEmpty(redirectUri))
        {
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
                throw new DomainException("Geçersiz redirect URI formatı");
        }
        
        if (!string.IsNullOrEmpty(scopes))
        {
            Guard.AgainstLength(scopes, 500, nameof(scopes));
        }
        
        if (!string.IsNullOrEmpty(settings))
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
        }

        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProviderKey = providerKey.ToLowerInvariant(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUri = redirectUri,
            IsActive = isActive,
            Scopes = scopes,
            Settings = settings,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        provider.AddDomainEvent(new OAuthProviderCreatedEvent(provider.Id, provider.Name, provider.ProviderKey));

        return provider;
    }

    // ✅ BOLUM 1.1: Domain Method - Update provider
    public void Update(
        string? name = null,
        string? clientId = null,
        string? clientSecret = null,
        string? redirectUri = null,
        string? scopes = null,
        string? settings = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 100, nameof(name));
            Name = name;
        }

        if (clientId != null)
        {
            Guard.AgainstNullOrEmpty(clientId, nameof(clientId));
            Guard.AgainstLength(clientId, 256, nameof(clientId));
            ClientId = clientId;
        }

        if (clientSecret != null)
        {
            Guard.AgainstNullOrEmpty(clientSecret, nameof(clientSecret));
            Guard.AgainstLength(clientSecret, 512, nameof(clientSecret));
            ClientSecret = clientSecret;
        }

        if (redirectUri != null)
        {
            if (string.IsNullOrEmpty(redirectUri))
            {
                RedirectUri = null;
            }
            else
            {
                if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
                    throw new DomainException("Geçersiz redirect URI formatı");
                RedirectUri = redirectUri;
            }
        }

        if (scopes != null)
        {
            Guard.AgainstLength(scopes, 500, nameof(scopes));
            Scopes = scopes;
        }
        
        if (settings != null)
        {
            Guard.AgainstLength(settings, 2000, nameof(settings));
            Settings = settings;
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OAuthProviderUpdatedEvent(Id, Name, ProviderKey));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate provider
    public void Activate()
    {
        if (IsActive)
            throw new DomainException("OAuth provider zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OAuthProviderActivatedEvent(Id, Name, ProviderKey));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate provider
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("OAuth provider zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OAuthProviderDeactivatedEvent(Id, Name, ProviderKey));
    }

    // ✅ BOLUM 1.1: Domain Method - Delete provider (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("OAuth provider zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new OAuthProviderDeletedEvent(Id, Name, ProviderKey));
    }
}

