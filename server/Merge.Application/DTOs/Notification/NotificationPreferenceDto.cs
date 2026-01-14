using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification Preference DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record NotificationPreferenceDto(
    Guid Id,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled,
    NotificationPreferenceSettingsDto? CustomSettings,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
