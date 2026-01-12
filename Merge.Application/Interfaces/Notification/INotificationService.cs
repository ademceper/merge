using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<NotificationDto?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}

