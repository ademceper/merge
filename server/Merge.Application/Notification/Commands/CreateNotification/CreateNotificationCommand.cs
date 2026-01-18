using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateNotification;


public record CreateNotificationCommand(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Link = null,
    string? Data = null) : IRequest<NotificationDto>;
