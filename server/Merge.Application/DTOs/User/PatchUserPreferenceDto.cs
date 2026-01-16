namespace Merge.Application.DTOs.User;

/// <summary>
/// Partial update DTO for User Preference (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchUserPreferenceDto
{
    public string? Theme { get; init; }
    public string? DefaultLanguage { get; init; }
    public string? DefaultCurrency { get; init; }
    public int? ItemsPerPage { get; init; }
    public string? DateFormat { get; init; }
    public string? TimeFormat { get; init; }
    public bool? EmailNotifications { get; init; }
    public bool? SmsNotifications { get; init; }
    public bool? PushNotifications { get; init; }
    public bool? OrderUpdates { get; init; }
    public bool? PromotionalEmails { get; init; }
    public bool? ProductRecommendations { get; init; }
    public bool? ReviewReminders { get; init; }
    public bool? WishlistPriceAlerts { get; init; }
    public bool? NewsletterSubscription { get; init; }
    public bool? ShowProfilePublicly { get; init; }
    public bool? ShowPurchaseHistory { get; init; }
    public bool? AllowPersonalization { get; init; }
    public bool? AllowDataCollection { get; init; }
    public bool? AllowThirdPartySharing { get; init; }
    public string? DefaultShippingAddress { get; init; }
    public string? DefaultPaymentMethod { get; init; }
    public bool? AutoApplyCoupons { get; init; }
    public bool? SaveCartOnLogout { get; init; }
    public bool? ShowOutOfStockItems { get; init; }
}
