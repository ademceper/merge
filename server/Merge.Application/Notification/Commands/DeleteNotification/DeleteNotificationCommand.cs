using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeleteNotification;

/// <summary>
/// Delete Notification Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
