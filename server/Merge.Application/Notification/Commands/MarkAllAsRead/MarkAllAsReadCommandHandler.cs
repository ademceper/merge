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

/// <summary>
/// Mark All Notifications As Read Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class MarkAllAsReadCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkAllAsReadCommandHandler> logger) : IRequestHandler<MarkAllAsReadCommand, bool>
{

    public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notifications = await context.Set<NotificationEntity>()
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
        if (notifications.Count == 0)
        {
            return true;
        }

        foreach (var notification in notifications)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            notification.MarkAsRead();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Tüm bildirimler okundu olarak işaretlendi. UserId: {UserId}, Count: {Count}",
            request.UserId, notifications.Count);

        return true;
    }
}
