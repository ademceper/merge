using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserNotifications;

/// <summary>
/// Get User Notifications Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public record GetUserNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;
