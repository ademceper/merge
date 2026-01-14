using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
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
