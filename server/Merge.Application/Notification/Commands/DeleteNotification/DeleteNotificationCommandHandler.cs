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

namespace Merge.Application.Notification.Commands.DeleteNotification;


public class DeleteNotificationCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteNotificationCommandHandler> logger) : IRequestHandler<DeleteNotificationCommand, bool>
{

    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
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

        notification.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification silindi. NotificationId: {NotificationId}, UserId: {UserId}",
            request.NotificationId, request.UserId);

        return true;
    }
}
