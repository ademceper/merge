using Merge.Domain.Modules.Notifications;
namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Update Notification Preference DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record UpdateNotificationPreferenceDto(
    bool? IsEnabled = null,
    NotificationPreferenceSettingsDto? CustomSettings = null);
