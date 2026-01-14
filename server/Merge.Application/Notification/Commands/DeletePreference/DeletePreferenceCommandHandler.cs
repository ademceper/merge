using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.DeletePreference;

/// <summary>
/// Delete Preference Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class DeletePreferenceCommandHandler : IRequestHandler<DeletePreferenceCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePreferenceCommandHandler> _logger;

    public DeletePreferenceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeletePreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeletePreferenceCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        if (preference == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        preference.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification preference silindi. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.NotificationType, request.Channel);

        return true;
    }
}
