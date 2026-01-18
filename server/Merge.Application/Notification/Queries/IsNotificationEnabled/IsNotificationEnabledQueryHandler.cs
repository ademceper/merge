using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.IsNotificationEnabled;


public class IsNotificationEnabledQueryHandler(IDbContext context) : IRequestHandler<IsNotificationEnabledQuery, bool>
{

    public async Task<bool> Handle(IsNotificationEnabledQuery request, CancellationToken cancellationToken)
    {
        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        // If no preference exists, default to enabled
        return preference?.IsEnabled ?? true;
    }
}
