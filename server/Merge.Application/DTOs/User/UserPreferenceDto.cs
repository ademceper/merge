using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
namespace Merge.Application.DTOs.User;

public record UserPreferenceDto(
    Guid UserId,
    // Display Preferences
    string Theme,
    string DefaultLanguage,
    string DefaultCurrency,
    int ItemsPerPage,
    string DateFormat,
    string TimeFormat,
    // Notification Preferences
    bool EmailNotifications,
    bool SmsNotifications,
    bool PushNotifications,
    bool OrderUpdates,
    bool PromotionalEmails,
    bool ProductRecommendations,
    bool ReviewReminders,
    bool WishlistPriceAlerts,
    bool NewsletterSubscription,
    // Privacy Preferences
    bool ShowProfilePublicly,
    bool ShowPurchaseHistory,
    bool AllowPersonalization,
    bool AllowDataCollection,
    bool AllowThirdPartySharing,
    // Shopping Preferences
    string DefaultShippingAddress,
    string DefaultPaymentMethod,
    bool AutoApplyCoupons,
    bool SaveCartOnLogout,
    bool ShowOutOfStockItems);
