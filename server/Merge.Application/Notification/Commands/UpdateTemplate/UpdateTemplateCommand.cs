using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.UpdateTemplate;


public record UpdateTemplateCommand(
    Guid Id,
    UpdateNotificationTemplateDto Dto) : IRequest<NotificationTemplateDto>;
