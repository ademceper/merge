using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Notification.Queries.IsNotificationEnabled;

/// <summary>
/// Is Notification Enabled Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class IsNotificationEnabledQueryHandler : IRequestHandler<IsNotificationEnabledQuery, bool>
{
    private readonly IDbContext _context;

    public IsNotificationEnabledQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsNotificationEnabledQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        // If no preference exists, default to enabled
        return preference?.IsEnabled ?? true;
    }
}
