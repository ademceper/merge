using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record NotificationPreferenceDto(
    Guid Id,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled,
    NotificationPreferenceSettingsDto? CustomSettings,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
