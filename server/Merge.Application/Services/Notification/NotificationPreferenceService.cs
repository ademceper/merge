using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Notification;

public class NotificationPreferenceService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<NotificationPreferenceService> logger) : INotificationPreferenceService
{

    public async Task<NotificationPreferenceDto> CreatePreferenceAsync(Guid userId, CreateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification preference oluşturuluyor. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        // Check if preference already exists
        var existing = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == dto.NotificationType && 
                                  np.Channel == dto.Channel, cancellationToken);

        if (existing is not null)
        {
            existing.Update(
                dto.IsEnabled,
                dto.CustomSettings is not null ? JsonSerializer.Serialize(dto.CustomSettings) : null);
        }
        else
        {
            var preference = NotificationPreference.Create(
                userId,
                dto.NotificationType,
                dto.Channel,
                dto.IsEnabled,
                dto.CustomSettings is not null ? JsonSerializer.Serialize(dto.CustomSettings) : null);

            await context.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdPreference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                      np.NotificationType == dto.NotificationType && 
                                      np.Channel == dto.Channel, cancellationToken);

        if (createdPreference is null)
        {
            throw new BusinessException("Tercih oluşturulamadı.");
        }

        logger.LogInformation(
            "Notification preference oluşturuldu. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        return mapper.Map<NotificationPreferenceDto>(createdPreference);
    }

    public async Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return null;
        }

        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        return preference is not null ? mapper.Map<NotificationPreferenceDto>(preference) : null;
    }

    public async Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .OrderBy(np => np.NotificationType)
            .ThenBy(np => np.Channel)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<NotificationPreferenceDto>>(preferences);
    }

    public async Task<NotificationPreferenceSummaryDto> GetUserPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .ToListAsync(cancellationToken);

        Dictionary<string, Dictionary<string, bool>> preferencesDict = [];
        int totalEnabled = 0;
        int totalDisabled = 0;

        foreach (var pref in preferences)
        {
            var notificationTypeStr = pref.NotificationType.ToString();
            var channelStr = pref.Channel.ToString();

            if (!preferencesDict.ContainsKey(notificationTypeStr))
            {
                preferencesDict[notificationTypeStr] = [];
            }
            preferencesDict[notificationTypeStr][channelStr] = pref.IsEnabled;

            if (pref.IsEnabled)
                totalEnabled++;
            else
                totalDisabled++;
        }

        var summary = new NotificationPreferenceSummaryDto(
            userId,
            preferencesDict,
            totalEnabled,
            totalDisabled);

        return summary;
    }

    public async Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, string notificationType, string channel, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        preference.Update(
            dto.IsEnabled ?? preference.IsEnabled,
            dto.CustomSettings is not null ? JsonSerializer.Serialize(dto.CustomSettings) : null);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<NotificationPreferenceDto>(preference);
    }

    public async Task<bool> DeletePreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return false;
        }

        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        if (preference is null) return false;

        preference.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> BulkUpdatePreferencesAsync(Guid userId, BulkUpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Preferences is null || dto.Preferences.Count == 0)
        {
            return true;
        }

        var notificationTypes = dto.Preferences.Select(p => p.NotificationType).Distinct().ToList();
        var channels = dto.Preferences.Select(p => p.Channel).Distinct().ToList();
        
        // Not: Bu minimal bir işlem ve business logic için gerekli (key matching için)
        var existingPreferencesList = await context.Set<NotificationPreference>()
            .Where(np => np.UserId == userId && 
                        notificationTypes.Contains(np.NotificationType) && 
                        channels.Contains(np.Channel))
            .ToListAsync(cancellationToken);
        
        var existingPreferences = existingPreferencesList
            .ToDictionary(np => new { np.NotificationType, np.Channel }, np => np);

        List<NotificationPreference> preferencesToAdd = [];
        List<NotificationPreference> preferencesToUpdate = [];

        foreach (var prefDto in dto.Preferences)
        {
            var key = new { prefDto.NotificationType, prefDto.Channel };
            if (existingPreferences.TryGetValue(key, out var existing))
            {
                existing.Update(
                    prefDto.IsEnabled,
                    prefDto.CustomSettings is not null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
                preferencesToUpdate.Add(existing);
            }
            else
            {
                var preference = NotificationPreference.Create(
                    userId,
                    prefDto.NotificationType,
                    prefDto.Channel,
                    prefDto.IsEnabled,
                    prefDto.CustomSettings is not null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
                preferencesToAdd.Add(preference);
            }
        }

        if (preferencesToAdd.Count > 0)
        {
            await context.Set<NotificationPreference>().AddRangeAsync(preferencesToAdd, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return false;
        }

        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        // If no preference exists, default to enabled
        return preference?.IsEnabled ?? true;
    }

    public async Task<IEnumerable<string>> GetEnabledChannelsAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum))
        {
            return [];
        }

        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId && 
                   np.NotificationType == notificationTypeEnum && 
                   np.IsEnabled)
            .Select(np => np.Channel.ToString()) // ✅ BOLUM 1.2: Enum.ToString() kullanıyoruz
            .ToListAsync(cancellationToken);

        return preferences;
    }
}

