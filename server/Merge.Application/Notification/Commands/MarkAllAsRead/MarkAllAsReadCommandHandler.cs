using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;


public class MarkAllAsReadCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkAllAsReadCommandHandler> logger) : IRequestHandler<MarkAllAsReadCommand, bool>
{

    public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = await context.Set<NotificationEntity>()
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return true;
        }

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Tüm bildirimler okundu olarak işaretlendi. UserId: {UserId}, Count: {Count}",
            request.UserId, notifications.Count);

        return true;
    }
}
