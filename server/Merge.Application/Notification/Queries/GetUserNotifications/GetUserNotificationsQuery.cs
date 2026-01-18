using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserNotifications;


public record GetUserNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;
