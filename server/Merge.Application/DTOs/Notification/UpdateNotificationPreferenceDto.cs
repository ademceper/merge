using Merge.Domain.Modules.Notifications;
namespace Merge.Application.DTOs.Notification;


public record UpdateNotificationPreferenceDto(
    bool? IsEnabled = null,
    NotificationPreferenceSettingsDto? CustomSettings = null);
