using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// OAuthAccount Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OAuthAccount : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
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

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private OAuthAccount() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
        Guard.AgainstNullOrEmpty(providerUserId, nameof(providerUserId));

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

        return oauthAccount;
    }

    // ✅ BOLUM 1.1: Domain Method - Update tokens
    public void UpdateTokens(string? accessToken, string? refreshToken, DateTime? tokenExpiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Set as primary
    public void SetAsPrimary()
    {
        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Remove primary status
    public void RemovePrimaryStatus()
    {
        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Computed property
    public bool IsTokenExpired => TokenExpiresAt.HasValue && DateTime.UtcNow >= TokenExpiresAt.Value;
}

