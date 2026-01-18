using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUnreadCount;


public record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
