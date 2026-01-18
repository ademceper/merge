using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.MarkAsRead;


public record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
