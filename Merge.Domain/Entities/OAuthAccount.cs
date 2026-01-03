namespace Merge.Domain.Entities;

/// <summary>
/// OAuthAccount Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OAuthAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // google, facebook, apple
    public string ProviderUserId { get; set; } = string.Empty; // External user ID from provider
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? PictureUrl { get; set; }
    public string? AccessToken { get; set; } // Encrypted
    public string? RefreshToken { get; set; } // Encrypted
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsPrimary { get; set; } = false; // Primary login method
    
    // Navigation properties
    public User User { get; set; } = null!;
}

