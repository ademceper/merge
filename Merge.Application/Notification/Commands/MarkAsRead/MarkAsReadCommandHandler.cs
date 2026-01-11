using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Entities.Notification;

namespace Merge.Application.Notification.Commands.MarkAsRead;

/// <summary>
/// Mark Notification As Read Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAsReadCommandHandler> _logger;

    public MarkAsReadCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkAsReadCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notification = await _context.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            _logger.LogWarning(
                "Notification bulunamadı. NotificationId: {NotificationId}, UserId: {UserId}",
                request.NotificationId, request.UserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        notification.MarkAsRead();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification okundu olarak işaretlendi. NotificationId: {NotificationId}, UserId: {UserId}",
            request.NotificationId, request.UserId);

        return true;
    }
}
