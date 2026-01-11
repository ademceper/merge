using MediatR;

namespace Merge.Application.Notification.Commands.MarkAsRead;

/// <summary>
/// Mark Notification As Read Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
