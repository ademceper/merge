using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;

/// <summary>
/// Mark All Notifications As Read Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record MarkAllAsReadCommand(Guid UserId) : IRequest<bool>;
