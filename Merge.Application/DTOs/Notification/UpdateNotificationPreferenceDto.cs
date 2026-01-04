using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class UpdateNotificationPreferenceDto
{
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Bildirim tercihi ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public NotificationPreferenceSettingsDto? CustomSettings { get; set; }
}
