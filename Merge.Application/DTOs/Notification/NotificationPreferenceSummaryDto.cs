namespace Merge.Application.DTOs.Notification;

public class NotificationPreferenceSummaryDto
{
    public Guid UserId { get; set; }
    public Dictionary<string, Dictionary<string, bool>> Preferences { get; set; } = new(); // NotificationType -> Channel -> IsEnabled
    public int TotalEnabled { get; set; }
    public int TotalDisabled { get; set; }
}
