using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;


public record MarkAllAsReadCommand(Guid UserId) : IRequest<bool>;
