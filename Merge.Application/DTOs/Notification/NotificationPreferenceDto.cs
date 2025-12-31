namespace Merge.Application.DTOs.Notification;

public class NotificationPreferenceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
