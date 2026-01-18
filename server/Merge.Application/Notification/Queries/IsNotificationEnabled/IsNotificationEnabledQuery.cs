using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.IsNotificationEnabled;


public record IsNotificationEnabledQuery(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<bool>;
