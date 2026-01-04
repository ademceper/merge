using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class CreateNotificationPreferenceDto
{
    [Required]
    [StringLength(100)]
    public string NotificationType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Channel { get; set; } = string.Empty;
    
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Bildirim tercihi ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public NotificationPreferenceSettingsDto? CustomSettings { get; set; }
}
