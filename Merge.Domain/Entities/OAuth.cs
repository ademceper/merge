using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

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

public class PushNotificationDevice : BaseEntity
{
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty; // FCM token or APNS token
    public string Platform { get; set; } = string.Empty; // iOS, Android, Web
    public string? DeviceId { get; set; } // Unique device identifier
    public string? DeviceModel { get; set; }
    public string? AppVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}

public class PushNotification : BaseEntity
{
    public Guid? UserId { get; set; } // Null for broadcast notifications
    public Guid? DeviceId { get; set; } // Specific device, null for all user devices
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Data { get; set; } // JSON data payload
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string NotificationType { get; set; } = string.Empty; // Order, Shipping, Promotion, etc.
    
    // Navigation properties
    public User? User { get; set; }
    public PushNotificationDevice? Device { get; set; }
}

public class InternationalShipping : BaseEntity
{
    public Guid OrderId { get; set; }
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public string? OriginCity { get; set; }
    public string? DestinationCity { get; set; }
    public string ShippingMethod { get; set; } = string.Empty; // Express, Standard, Economy
    public decimal ShippingCost { get; set; }
    public decimal? CustomsDuty { get; set; }
    public decimal? ImportTax { get; set; }
    public decimal? HandlingFee { get; set; }
    public decimal TotalCost { get; set; }
    public int EstimatedDays { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CustomsDeclarationNumber { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    // Not: ShippingStatus enum'u var ama InternationalShipping için özel değerler gerekebilir
    // Şimdilik ShippingStatus kullanıyoruz, gerekirse yeni enum oluşturulabilir
    public ShippingStatus Status { get; set; } = ShippingStatus.Preparing;
    public DateTime? ShippedAt { get; set; }
    public DateTime? InCustomsAt { get; set; }
    public DateTime? ClearedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

public class TaxRule : BaseEntity
{
    public string Country { get; set; } = string.Empty;
    public string? State { get; set; } // For countries with state-level tax
    public string? City { get; set; } // For cities with local tax
    public string TaxType { get; set; } = string.Empty; // VAT, GST, Sales Tax, etc.
    public decimal TaxRate { get; set; } // Percentage (e.g., 20 for 20%)
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ProductCategoryIds { get; set; } // Comma separated category IDs (null = all categories)
    public bool IsInclusive { get; set; } = false; // Tax included in price or added on top
    public string? Notes { get; set; }
}

public class CustomsDeclaration : BaseEntity
{
    public Guid OrderId { get; set; }
    public string DeclarationNumber { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = "USD";
    public string? HsCode { get; set; } // Harmonized System code
    public string? Description { get; set; }
    public decimal Weight { get; set; } // in kg
    public int Quantity { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? CustomsDuty { get; set; }
    public decimal? ImportTax { get; set; }
    public string? Documents { get; set; } // JSON array of document URLs
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

