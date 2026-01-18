using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplates;


public record GetTemplatesQuery(
    NotificationType? Type = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationTemplateDto>>;
