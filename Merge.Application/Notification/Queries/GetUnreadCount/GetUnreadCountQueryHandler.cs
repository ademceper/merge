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

/// <summary>
/// Get Unread Count Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IDbContext _context;

    public GetUnreadCountQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        return await _context.Set<NotificationEntity>()
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);
    }
}
