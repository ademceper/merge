using Merge.Domain.SharedKernel;
namespace Merge.Domain.Modules.Identity;

/// <summary>
/// OAuthProvider Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OAuthProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Google, Facebook, Apple
    public string ProviderKey { get; set; } = string.Empty; // google, facebook, apple
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty; // Encrypted
    public string? RedirectUri { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Scopes { get; set; } // Comma separated scopes
    public string? Settings { get; set; } // JSON for provider-specific settings
}

