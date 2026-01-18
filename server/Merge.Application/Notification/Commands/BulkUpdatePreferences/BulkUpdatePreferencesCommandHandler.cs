using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.BulkUpdatePreferences;


public class BulkUpdatePreferencesCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<BulkUpdatePreferencesCommandHandler> logger) : IRequestHandler<BulkUpdatePreferencesCommand, bool>
{

    public async Task<bool> Handle(BulkUpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.Preferences == null || request.Dto.Preferences.Count == 0)
        {
            return true;
        }

        var notificationTypes = request.Dto.Preferences.Select(p => p.NotificationType).Distinct().ToList();
        var channels = request.Dto.Preferences.Select(p => p.Channel).Distinct().ToList();
        
        var existingPreferencesList = await context.Set<NotificationPreference>()
            .Where(np => np.UserId == request.UserId && 
                        notificationTypes.Contains(np.NotificationType) && 
                        channels.Contains(np.Channel))
            .ToListAsync(cancellationToken);
        
        var existingPreferences = existingPreferencesList
            .ToDictionary(np => new { np.NotificationType, np.Channel }, np => np);

        List<NotificationPreference> preferencesToAdd = [];

        foreach (var prefDto in request.Dto.Preferences)
        {
            var key = new { prefDto.NotificationType, prefDto.Channel };
            if (existingPreferences.TryGetValue(key, out var existing))
            {
                existing.Update(
                    prefDto.IsEnabled,
                    prefDto.CustomSettings != null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
            }
            else
            {
                var preference = NotificationPreference.Create(
                    request.UserId,
                    prefDto.NotificationType,
                    prefDto.Channel,
                    prefDto.IsEnabled,
                    prefDto.CustomSettings != null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
                preferencesToAdd.Add(preference);
            }
        }

        if (preferencesToAdd.Count > 0)
        {
            await context.Set<NotificationPreference>().AddRangeAsync(preferencesToAdd, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Toplu notification preference güncellendi. UserId: {UserId}, Count: {Count}",
            request.UserId, request.Dto.Preferences.Count);

        return true;
    }
}
