using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.UpdateUserPreference;

public record UpdateUserPreferenceCommand(
    Guid UserId,
    string? Theme,
    string? DefaultLanguage,
    string? DefaultCurrency,
    int? ItemsPerPage,
    string? DateFormat,
    string? TimeFormat,
    bool? EmailNotifications,
    bool? SmsNotifications,
    bool? PushNotifications,
    bool? OrderUpdates,
    bool? PromotionalEmails,
    bool? ProductRecommendations,
    bool? ReviewReminders,
    bool? WishlistPriceAlerts,
    bool? NewsletterSubscription,
    bool? ShowProfilePublicly,
    bool? ShowPurchaseHistory,
    bool? AllowPersonalization,
    bool? AllowDataCollection,
    bool? AllowThirdPartySharing,
    string? DefaultShippingAddress,
    string? DefaultPaymentMethod,
    bool? AutoApplyCoupons,
    bool? SaveCartOnLogout,
    bool? ShowOutOfStockItems
) : IRequest<UserPreferenceDto>;
