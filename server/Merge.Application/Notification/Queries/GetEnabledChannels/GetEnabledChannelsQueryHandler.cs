using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetEnabledChannels;


public class GetEnabledChannelsQueryHandler(IDbContext context) : IRequestHandler<GetEnabledChannelsQuery, IEnumerable<string>>
{

    public async Task<IEnumerable<string>> Handle(GetEnabledChannelsQuery request, CancellationToken cancellationToken)
    {
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == request.UserId && 
                   np.NotificationType == request.NotificationType && 
                   np.IsEnabled)
            .Select(np => np.Channel.ToString())
            .ToListAsync(cancellationToken);

        return preferences;
    }
}
