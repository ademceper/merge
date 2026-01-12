using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Notifications;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// UserPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserPreference : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }

    // Display Preferences
    public string Theme { get; private set; } = "Light"; // Light, Dark, Auto
    public string DefaultLanguage { get; private set; } = "tr-TR";
    public string DefaultCurrency { get; private set; } = "TRY";
    public int ItemsPerPage { get; private set; } = 20;
    public string DateFormat { get; private set; } = "dd/MM/yyyy";
    public string TimeFormat { get; private set; } = "24h"; // 12h, 24h

    // Notification Preferences
    public bool EmailNotifications { get; private set; } = true;
    public bool SmsNotifications { get; private set; } = false;
    public bool PushNotifications { get; private set; } = true;
    public bool OrderUpdates { get; private set; } = true;
    public bool PromotionalEmails { get; private set; } = true;
    public bool ProductRecommendations { get; private set; } = true;
    public bool ReviewReminders { get; private set; } = true;
    public bool WishlistPriceAlerts { get; private set; } = true;
    public bool NewsletterSubscription { get; private set; } = false;

    // Privacy Preferences
    public bool ShowProfilePublicly { get; private set; } = false;
    public bool ShowPurchaseHistory { get; private set; } = false;
    public bool AllowPersonalization { get; private set; } = true;
    public bool AllowDataCollection { get; private set; } = true;
    public bool AllowThirdPartySharing { get; private set; } = false;

    // Shopping Preferences
    public string DefaultShippingAddress { get; private set; } = string.Empty; // Address ID
    public string DefaultPaymentMethod { get; private set; } = string.Empty;
    public bool AutoApplyCoupons { get; private set; } = true;
    public bool SaveCartOnLogout { get; private set; } = true;
    public bool ShowOutOfStockItems { get; private set; } = false;

    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private UserPreference() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static UserPreference Create(Guid userId)
    {
        Guard.AgainstDefault(userId, nameof(userId));

        var preference = new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        preference.AddDomainEvent(new UserPreferenceCreatedEvent(preference.Id, userId));

        return preference;
    }

    // ✅ BOLUM 1.1: Domain Method - Update preferences
    public void UpdatePreferences(
        string? theme = null,
        string? defaultLanguage = null,
        string? defaultCurrency = null,
        int? itemsPerPage = null,
        string? dateFormat = null,
        string? timeFormat = null,
        bool? emailNotifications = null,
        bool? smsNotifications = null,
        bool? pushNotifications = null,
        bool? orderUpdates = null,
        bool? promotionalEmails = null,
        bool? productRecommendations = null,
        bool? reviewReminders = null,
        bool? wishlistPriceAlerts = null,
        bool? newsletterSubscription = null,
        bool? showProfilePublicly = null,
        bool? showPurchaseHistory = null,
        bool? allowPersonalization = null,
        bool? allowDataCollection = null,
        bool? allowThirdPartySharing = null,
        string? defaultShippingAddress = null,
        string? defaultPaymentMethod = null,
        bool? autoApplyCoupons = null,
        bool? saveCartOnLogout = null,
        bool? showOutOfStockItems = null)
    {
        if (itemsPerPage.HasValue && (itemsPerPage.Value < 1 || itemsPerPage.Value > 100))
            throw new DomainException("ItemsPerPage must be between 1 and 100");

        if (theme != null) Theme = theme;
        if (defaultLanguage != null) DefaultLanguage = defaultLanguage;
        if (defaultCurrency != null) DefaultCurrency = defaultCurrency;
        if (itemsPerPage.HasValue) ItemsPerPage = itemsPerPage.Value;
        if (dateFormat != null) DateFormat = dateFormat;
        if (timeFormat != null) TimeFormat = timeFormat;

        if (emailNotifications.HasValue) EmailNotifications = emailNotifications.Value;
        if (smsNotifications.HasValue) SmsNotifications = smsNotifications.Value;
        if (pushNotifications.HasValue) PushNotifications = pushNotifications.Value;
        if (orderUpdates.HasValue) OrderUpdates = orderUpdates.Value;
        if (promotionalEmails.HasValue) PromotionalEmails = promotionalEmails.Value;
        if (productRecommendations.HasValue) ProductRecommendations = productRecommendations.Value;
        if (reviewReminders.HasValue) ReviewReminders = reviewReminders.Value;
        if (wishlistPriceAlerts.HasValue) WishlistPriceAlerts = wishlistPriceAlerts.Value;
        if (newsletterSubscription.HasValue) NewsletterSubscription = newsletterSubscription.Value;

        if (showProfilePublicly.HasValue) ShowProfilePublicly = showProfilePublicly.Value;
        if (showPurchaseHistory.HasValue) ShowPurchaseHistory = showPurchaseHistory.Value;
        if (allowPersonalization.HasValue) AllowPersonalization = allowPersonalization.Value;
        if (allowDataCollection.HasValue) AllowDataCollection = allowDataCollection.Value;
        if (allowThirdPartySharing.HasValue) AllowThirdPartySharing = allowThirdPartySharing.Value;

        if (defaultShippingAddress != null) DefaultShippingAddress = defaultShippingAddress;
        if (defaultPaymentMethod != null) DefaultPaymentMethod = defaultPaymentMethod;
        if (autoApplyCoupons.HasValue) AutoApplyCoupons = autoApplyCoupons.Value;
        if (saveCartOnLogout.HasValue) SaveCartOnLogout = saveCartOnLogout.Value;
        if (showOutOfStockItems.HasValue) ShowOutOfStockItems = showOutOfStockItems.Value;

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new UserPreferenceUpdatedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Reset to defaults
    public void ResetToDefaults()
    {
        Theme = "Light";
        DefaultLanguage = "tr-TR";
        DefaultCurrency = "TRY";
        ItemsPerPage = 20;
        DateFormat = "dd/MM/yyyy";
        TimeFormat = "24h";

        EmailNotifications = true;
        SmsNotifications = false;
        PushNotifications = true;
        OrderUpdates = true;
        PromotionalEmails = true;
        ProductRecommendations = true;
        ReviewReminders = true;
        WishlistPriceAlerts = true;
        NewsletterSubscription = false;

        ShowProfilePublicly = false;
        ShowPurchaseHistory = false;
        AllowPersonalization = true;
        AllowDataCollection = true;
        AllowThirdPartySharing = false;

        DefaultShippingAddress = string.Empty;
        DefaultPaymentMethod = string.Empty;
        AutoApplyCoupons = true;
        SaveCartOnLogout = true;
        ShowOutOfStockItems = false;

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        AddDomainEvent(new UserPreferenceUpdatedEvent(Id, UserId));
    }
}
