using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.UpdateTemplate;

/// <summary>
/// Update Template Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record UpdateTemplateCommand(
    Guid Id,
    UpdateNotificationTemplateDto Dto) : IRequest<NotificationTemplateDto>;
