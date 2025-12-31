using Merge.Application.DTOs.Notification;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

