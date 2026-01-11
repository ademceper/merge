using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Entities.Notification;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;

/// <summary>
/// Mark All Notifications As Read Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAllAsReadCommandHandler> _logger;

    public MarkAllAsReadCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkAllAsReadCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notifications = await _context.Set<NotificationEntity>()
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Tüm bildirimler okundu olarak işaretlendi. UserId: {UserId}, Count: {Count}",
            request.UserId, notifications.Count);

        return true;
    }
}
