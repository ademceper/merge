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

namespace Merge.Application.Notification.Commands.MarkAsRead;


public class MarkAsReadCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkAsReadCommandHandler> logger) : IRequestHandler<MarkAsReadCommand, bool>
{

    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification is null)
        {
            logger.LogWarning(
                "Notification bulunamadı. NotificationId: {NotificationId}, UserId: {UserId}",
                request.NotificationId, request.UserId);
            return false;
        }

        notification.MarkAsRead();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification okundu olarak işaretlendi. NotificationId: {NotificationId}, UserId: {UserId}",
            request.NotificationId, request.UserId);

        return true;
    }
}
