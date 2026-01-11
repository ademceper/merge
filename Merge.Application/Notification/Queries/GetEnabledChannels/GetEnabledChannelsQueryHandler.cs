using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Notification.Queries.GetEnabledChannels;

/// <summary>
/// Get Enabled Channels Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetEnabledChannelsQueryHandler : IRequestHandler<GetEnabledChannelsQuery, IEnumerable<string>>
{
    private readonly IDbContext _context;

    public GetEnabledChannelsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> Handle(GetEnabledChannelsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == request.UserId && 
                   np.NotificationType == request.NotificationType && 
                   np.IsEnabled)
            .Select(np => np.Channel.ToString())
            .ToListAsync(cancellationToken);

        return preferences;
    }
}
