namespace Merge.Application.DTOs.User;

public class UserPreferenceDto
{
    public Guid UserId { get; set; }

    // Display Preferences
    public string Theme { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public int ItemsPerPage { get; set; }
    public string DateFormat { get; set; } = string.Empty;
    public string TimeFormat { get; set; } = string.Empty;

    // Notification Preferences
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool OrderUpdates { get; set; }
    public bool PromotionalEmails { get; set; }
    public bool ProductRecommendations { get; set; }
    public bool ReviewReminders { get; set; }
    public bool WishlistPriceAlerts { get; set; }
    public bool NewsletterSubscription { get; set; }

    // Privacy Preferences
    public bool ShowProfilePublicly { get; set; }
    public bool ShowPurchaseHistory { get; set; }
    public bool AllowPersonalization { get; set; }
    public bool AllowDataCollection { get; set; }
    public bool AllowThirdPartySharing { get; set; }

    // Shopping Preferences
    public string DefaultShippingAddress { get; set; } = string.Empty;
    public string DefaultPaymentMethod { get; set; } = string.Empty;
    public bool AutoApplyCoupons { get; set; }
    public bool SaveCartOnLogout { get; set; }
    public bool ShowOutOfStockItems { get; set; }
}
