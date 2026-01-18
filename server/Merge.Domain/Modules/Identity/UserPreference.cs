using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Enums;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// UserPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string Theme, TimeFormat YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserPreference : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    // Display Preferences
    public Theme Theme { get; private set; } = Theme.Light;
    public string DefaultLanguage { get; private set; } = "tr-TR";
    public string DefaultCurrency { get; private set; } = "TRY";
    public int ItemsPerPage { get; private set; } = 20;
    public string DateFormat { get; private set; } = "dd/MM/yyyy";
    public TimeFormat TimeFormat { get; private set; } = TimeFormat.TwentyFourHour;

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

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private UserPreference() { }

    public static UserPreference Create(Guid userId)
    {
        Guard.AgainstDefault(userId, nameof(userId));

        var preference = new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        preference.AddDomainEvent(new UserPreferenceCreatedEvent(preference.Id, userId));

        return preference;
    }

    public void UpdatePreferences(
        Theme? theme = null,
        string? defaultLanguage = null,
        string? defaultCurrency = null,
        int? itemsPerPage = null,
        string? dateFormat = null,
        TimeFormat? timeFormat = null,
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

        if (theme.HasValue) Theme = theme.Value;
        
        if (defaultLanguage != null)
        {
            Guard.AgainstLength(defaultLanguage, 10, nameof(defaultLanguage));
            DefaultLanguage = defaultLanguage;
        }
        
        if (defaultCurrency != null)
        {
            Guard.AgainstLength(defaultCurrency, 10, nameof(defaultCurrency));
            DefaultCurrency = defaultCurrency;
        }
        
        if (itemsPerPage.HasValue) ItemsPerPage = itemsPerPage.Value;
        
        if (dateFormat != null)
        {
            Guard.AgainstLength(dateFormat, 50, nameof(dateFormat));
            DateFormat = dateFormat;
        }
        
        if (timeFormat.HasValue) TimeFormat = timeFormat.Value;

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

        if (defaultShippingAddress != null)
        {
            Guard.AgainstLength(defaultShippingAddress, 50, nameof(defaultShippingAddress));
            DefaultShippingAddress = defaultShippingAddress;
        }
        
        if (defaultPaymentMethod != null)
        {
            Guard.AgainstLength(defaultPaymentMethod, 50, nameof(defaultPaymentMethod));
            DefaultPaymentMethod = defaultPaymentMethod;
        }
        if (autoApplyCoupons.HasValue) AutoApplyCoupons = autoApplyCoupons.Value;
        if (saveCartOnLogout.HasValue) SaveCartOnLogout = saveCartOnLogout.Value;
        if (showOutOfStockItems.HasValue) ShowOutOfStockItems = showOutOfStockItems.Value;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPreferenceUpdatedEvent(Id, UserId));
    }

    public void ResetToDefaults()
    {
        Theme = Theme.Light;
        DefaultLanguage = "tr-TR";
        DefaultCurrency = "TRY";
        ItemsPerPage = 20;
        DateFormat = "dd/MM/yyyy";
        TimeFormat = TimeFormat.TwentyFourHour;

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

        AddDomainEvent(new UserPreferenceUpdatedEvent(Id, UserId));
    }
}
