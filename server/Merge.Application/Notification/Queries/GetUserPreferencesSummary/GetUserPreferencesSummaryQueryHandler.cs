using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetUserPreferencesSummary;

/// <summary>
/// Get User Preferences Summary Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetUserPreferencesSummaryQueryHandler(IDbContext context) : IRequestHandler<GetUserPreferencesSummaryQuery, NotificationPreferenceSummaryDto>
{

    public async Task<NotificationPreferenceSummaryDto> Handle(GetUserPreferencesSummaryQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var preferencesDict = new Dictionary<string, Dictionary<string, bool>>();
        int totalEnabled = 0;
        int totalDisabled = 0;

        foreach (var pref in preferences)
        {
            var typeKey = pref.NotificationType.ToString();
            var channelKey = pref.Channel.ToString();
            
            if (!preferencesDict.ContainsKey(typeKey))
            {
                preferencesDict[typeKey] = new Dictionary<string, bool>();
            }
            preferencesDict[typeKey][channelKey] = pref.IsEnabled;

            if (pref.IsEnabled)
                totalEnabled++;
            else
                totalDisabled++;
        }

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı
        return new NotificationPreferenceSummaryDto(
            request.UserId,
            preferencesDict,
            totalEnabled,
            totalDisabled);
    }
}
