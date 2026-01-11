using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Commands.CreateNotification;

/// <summary>
/// Create Notification Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record CreateNotificationCommand(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Link = null,
    string? Data = null) : IRequest<NotificationDto>;
