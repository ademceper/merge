using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record CreateNotificationPreferenceDto(
    [Required] NotificationType NotificationType,
    [Required] NotificationChannel Channel,
    bool IsEnabled = true,
    NotificationPreferenceSettingsDto? CustomSettings = null);
