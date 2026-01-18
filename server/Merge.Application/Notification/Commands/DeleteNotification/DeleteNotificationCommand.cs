using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeleteNotification;


public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
