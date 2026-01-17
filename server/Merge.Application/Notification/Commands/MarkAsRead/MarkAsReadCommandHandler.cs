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

/// <summary>
/// Mark Notification As Read Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class MarkAsReadCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkAsReadCommandHandler> logger) : IRequestHandler<MarkAsReadCommand, bool>
{

    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notification = await context.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            logger.LogWarning(
                "Notification bulunamadı. NotificationId: {NotificationId}, UserId: {UserId}",
                request.NotificationId, request.UserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        notification.MarkAsRead();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification okundu olarak işaretlendi. NotificationId: {NotificationId}, UserId: {UserId}",
            request.NotificationId, request.UserId);

        return true;
    }
}
