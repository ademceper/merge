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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto> CreatePreferenceAsync(Guid userId, CreateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification preference oluşturuluyor. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        // Check if preference already exists
        var existing = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == dto.NotificationType && 
                                  np.Channel == dto.Channel, cancellationToken);

        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Update(
                dto.IsEnabled,
                dto.CustomSettings != null ? JsonSerializer.Serialize(dto.CustomSettings) : null);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var preference = NotificationPreference.Create(
                userId,
                dto.NotificationType,
                dto.Channel,
                dto.IsEnabled,
                dto.CustomSettings != null ? JsonSerializer.Serialize(dto.CustomSettings) : null);

            await context.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var createdPreference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                      np.NotificationType == dto.NotificationType && 
                                      np.Channel == dto.Channel, cancellationToken);

        if (createdPreference == null)
        {
            throw new BusinessException("Tercih oluşturulamadı.");
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification preference oluşturuldu. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationPreferenceDto>(createdPreference);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return null;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return preference != null ? mapper.Map<NotificationPreferenceDto>(preference) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .OrderBy(np => np.NotificationType)
            .ThenBy(np => np.Channel)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        return mapper.Map<IEnumerable<NotificationPreferenceDto>>(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceSummaryDto> GetUserPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK) - Enum.ToString() kullanıyoruz
        var preferencesDict = new Dictionary<string, Dictionary<string, bool>>();
        int totalEnabled = 0;
        int totalDisabled = 0;

        foreach (var pref in preferences)
        {
            var notificationTypeStr = pref.NotificationType.ToString();
            var channelStr = pref.Channel.ToString();

            if (!preferencesDict.ContainsKey(notificationTypeStr))
            {
                preferencesDict[notificationTypeStr] = new Dictionary<string, bool>();
            }
            preferencesDict[notificationTypeStr][channelStr] = pref.IsEnabled;

            if (pref.IsEnabled)
                totalEnabled++;
            else
                totalDisabled++;
        }

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı
        var summary = new NotificationPreferenceSummaryDto(
            userId,
            preferencesDict,
            totalEnabled,
            totalDisabled);

        return summary;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, string notificationType, string channel, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        if (preference == null)
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        preference.Update(
            dto.IsEnabled ?? preference.IsEnabled,
            dto.CustomSettings != null ? JsonSerializer.Serialize(dto.CustomSettings) : null);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationPreferenceDto>(preference);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeletePreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        if (preference == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        preference.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> BulkUpdatePreferencesAsync(Guid userId, BulkUpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Preferences == null || dto.Preferences.Count == 0)
        {
            return true;
        }

        // ✅ PERFORMANCE: Batch load existing preferences (N+1 fix)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() ve Distinct() YASAK - DTO'dan gelen list üzerinde işlem yapılıyor, sorun yok
        var notificationTypes = dto.Preferences.Select(p => p.NotificationType).Distinct().ToList();
        var channels = dto.Preferences.Select(p => p.Channel).Distinct().ToList();
        
        // ✅ PERFORMANCE: Anonymous type için ToDictionaryAsync kullanılamaz, bu yüzden ToListAsync kullanıp memory'de ToDictionary yapıyoruz
        // Not: Bu minimal bir işlem ve business logic için gerekli (key matching için)
        var existingPreferencesList = await context.Set<NotificationPreference>()
            .Where(np => np.UserId == userId && 
                        notificationTypes.Contains(np.NotificationType) && 
                        channels.Contains(np.Channel))
            .ToListAsync(cancellationToken);
        
        var existingPreferences = existingPreferencesList
            .ToDictionary(np => new { np.NotificationType, np.Channel }, np => np);

        var preferencesToAdd = new List<NotificationPreference>();
        var preferencesToUpdate = new List<NotificationPreference>();

        foreach (var prefDto in dto.Preferences)
        {
            var key = new { prefDto.NotificationType, prefDto.Channel };
            if (existingPreferences.TryGetValue(key, out var existing))
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                existing.Update(
                    prefDto.IsEnabled,
                    prefDto.CustomSettings != null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
                preferencesToUpdate.Add(existing);
            }
            else
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                var preference = NotificationPreference.Create(
                    userId,
                    prefDto.NotificationType,
                    prefDto.Channel,
                    prefDto.IsEnabled,
                    prefDto.CustomSettings != null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
                preferencesToAdd.Add(preference);
            }
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
        if (preferencesToAdd.Count > 0)
        {
            await context.Set<NotificationPreference>().AddRangeAsync(preferencesToAdd, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<Merge.Domain.Enums.NotificationChannel>(channel, true, out var channelEnum))
        {
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationTypeEnum && 
                                  np.Channel == channelEnum, cancellationToken);

        // If no preference exists, default to enabled
        return preference?.IsEnabled ?? true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<string>> GetEnabledChannelsAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(notificationType, true, out var notificationTypeEnum))
        {
            return Enumerable.Empty<string>();
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
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

