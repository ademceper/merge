using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUnreadCount;

/// <summary>
/// Get Unread Count Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
