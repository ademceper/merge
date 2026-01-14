using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification preference ayarlari icin typed DTO - Dictionary yerine guvenli
/// BOLUM 7.1.5: Records (C# 12 modern features) - Opsiyonel ama tutarlılık için record kullanıyoruz
/// </summary>
public record NotificationPreferenceSettingsDto(
    bool EmailEnabled = true,
    bool SmsEnabled = false,
    bool PushEnabled = true,
    bool OrderNotifications = true,
    bool PromotionNotifications = true,
    bool StockNotifications = true,
    bool PriceChangeNotifications = false,
    bool WeeklySummary = true,
    TimeSpan? QuietHoursStart = null,
    TimeSpan? QuietHoursEnd = null,
    [StringLength(10)] string? PreferredLanguage = null,
    [StringLength(50)] string? Timezone = null);
