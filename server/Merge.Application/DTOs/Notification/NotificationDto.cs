using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record NotificationDto(
    Guid Id,
    Guid UserId, // IDOR kontrolü için gerekli
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime? ReadAt,
    string? Link,
    string? Data, // JSON formatında ek veriler
    DateTime CreatedAt);
