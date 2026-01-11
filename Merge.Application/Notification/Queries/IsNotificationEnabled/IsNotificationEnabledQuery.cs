using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Queries.IsNotificationEnabled;

/// <summary>
/// Is Notification Enabled Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record IsNotificationEnabledQuery(
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IRequest<bool>;
