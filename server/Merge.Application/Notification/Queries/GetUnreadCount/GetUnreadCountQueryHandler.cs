using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetUnreadCount;


public class GetUnreadCountQueryHandler(IDbContext context) : IRequestHandler<GetUnreadCountQuery, int>
{

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await context.Set<NotificationEntity>()
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);
    }
}
