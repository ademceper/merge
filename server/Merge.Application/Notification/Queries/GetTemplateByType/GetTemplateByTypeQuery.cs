using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplateByType;


public record GetTemplateByTypeQuery(NotificationType Type) : IRequest<NotificationTemplateDto?>;
