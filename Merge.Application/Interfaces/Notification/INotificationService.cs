using Merge.Application.DTOs.Notification;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 20);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

