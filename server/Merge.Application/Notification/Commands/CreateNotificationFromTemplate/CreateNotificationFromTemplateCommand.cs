using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;


public record CreateNotificationFromTemplateCommand(
    Guid UserId,
    NotificationType TemplateType,
    NotificationVariablesDto? Variables = null) : IRequest<NotificationDto>;
