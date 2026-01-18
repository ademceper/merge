using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplate;


public record GetTemplateQuery(Guid Id) : IRequest<NotificationTemplateDto?>;
