namespace Merge.Domain.Entities;

public class UserPreference : BaseEntity
{
    public Guid UserId { get; set; }

    // Display Preferences
    public string Theme { get; set; } = "Light"; // Light, Dark, Auto
    public string DefaultLanguage { get; set; } = "tr-TR";
    public string DefaultCurrency { get; set; } = "TRY";
    public int ItemsPerPage { get; set; } = 20;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h"; // 12h, 24h

    // Notification Preferences
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool OrderUpdates { get; set; } = true;
    public bool PromotionalEmails { get; set; } = true;
    public bool ProductRecommendations { get; set; } = true;
    public bool ReviewReminders { get; set; } = true;
    public bool WishlistPriceAlerts { get; set; } = true;
    public bool NewsletterSubscription { get; set; } = false;

    // Privacy Preferences
    public bool ShowProfilePublicly { get; set; } = false;
    public bool ShowPurchaseHistory { get; set; } = false;
    public bool AllowPersonalization { get; set; } = true;
    public bool AllowDataCollection { get; set; } = true;
    public bool AllowThirdPartySharing { get; set; } = false;

    // Shopping Preferences
    public string DefaultShippingAddress { get; set; } = string.Empty; // Address ID
    public string DefaultPaymentMethod { get; set; } = string.Empty;
    public bool AutoApplyCoupons { get; set; } = true;
    public bool SaveCartOnLogout { get; set; } = true;
    public bool ShowOutOfStockItems { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
}
