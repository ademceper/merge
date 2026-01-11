using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;

/// <summary>
/// Create Notification From Template Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record CreateNotificationFromTemplateCommand(
    Guid UserId,
    NotificationType TemplateType,
    NotificationVariablesDto? Variables = null) : IRequest<NotificationDto>;
