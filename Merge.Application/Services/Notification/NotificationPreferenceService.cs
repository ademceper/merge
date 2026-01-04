using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = Merge.Domain.Entities.Notification;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Notification;


namespace Merge.Application.Services.Notification;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationPreferenceService> _logger;

    public NotificationPreferenceService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<NotificationPreferenceService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto> CreatePreferenceAsync(Guid userId, CreateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification preference oluşturuluyor. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        // Check if preference already exists
        var existing = await _context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == dto.NotificationType && 
                                  np.Channel == dto.Channel, cancellationToken);

        if (existing != null)
        {
            existing.IsEnabled = dto.IsEnabled;
            existing.CustomSettings = dto.CustomSettings != null 
                ? JsonSerializer.Serialize(dto.CustomSettings) 
                : null;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var preference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = dto.NotificationType,
                Channel = dto.Channel,
                IsEnabled = dto.IsEnabled,
                CustomSettings = dto.CustomSettings != null 
                    ? JsonSerializer.Serialize(dto.CustomSettings) 
                    : null
            };

            await _context.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var createdPreference = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                      np.NotificationType == dto.NotificationType && 
                                      np.Channel == dto.Channel, cancellationToken);

        if (createdPreference == null)
        {
            throw new BusinessException("Tercih oluşturulamadı.");
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification preference oluşturuldu. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            userId, dto.NotificationType, dto.Channel);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationPreferenceDto>(createdPreference);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationType && 
                                  np.Channel == channel, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return preference != null ? _mapper.Map<NotificationPreferenceDto>(preference) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .OrderBy(np => np.NotificationType)
            .ThenBy(np => np.Channel)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<NotificationPreferenceDto>>(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceSummaryDto> GetUserPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId)
            .ToListAsync(cancellationToken);

        var summary = new NotificationPreferenceSummaryDto
        {
            UserId = userId,
            Preferences = new Dictionary<string, Dictionary<string, bool>>()
        };

        foreach (var pref in preferences)
        {
            if (!summary.Preferences.ContainsKey(pref.NotificationType))
            {
                summary.Preferences[pref.NotificationType] = new Dictionary<string, bool>();
            }
            summary.Preferences[pref.NotificationType][pref.Channel] = pref.IsEnabled;

            if (pref.IsEnabled)
                summary.TotalEnabled++;
            else
                summary.TotalDisabled++;
        }

        return summary;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, string notificationType, string channel, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationType && 
                                  np.Channel == channel, cancellationToken);

        if (preference == null)
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        if (dto.IsEnabled.HasValue)
            preference.IsEnabled = dto.IsEnabled.Value;
        if (dto.CustomSettings != null)
            preference.CustomSettings = JsonSerializer.Serialize(dto.CustomSettings);

        preference.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationPreferenceDto>(preference);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeletePreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationType && 
                                  np.Channel == channel, cancellationToken);

        if (preference == null) return false;

        preference.IsDeleted = true;
        preference.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var existingPreferencesList = await _context.Set<NotificationPreference>()
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
                existing.IsEnabled = prefDto.IsEnabled;
                existing.CustomSettings = prefDto.CustomSettings != null 
                    ? JsonSerializer.Serialize(prefDto.CustomSettings) 
                    : null;
                existing.UpdatedAt = DateTime.UtcNow;
                preferencesToUpdate.Add(existing);
            }
            else
            {
                var preference = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = prefDto.NotificationType,
                    Channel = prefDto.Channel,
                    IsEnabled = prefDto.IsEnabled,
                    CustomSettings = prefDto.CustomSettings != null 
                        ? JsonSerializer.Serialize(prefDto.CustomSettings) 
                        : null
                };
                preferencesToAdd.Add(preference);
            }
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
        if (preferencesToAdd.Count > 0)
        {
            await _context.Set<NotificationPreference>().AddRangeAsync(preferencesToAdd, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && 
                                  np.NotificationType == notificationType && 
                                  np.Channel == channel, cancellationToken);

        // If no preference exists, default to enabled
        return preference?.IsEnabled ?? true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<string>> GetEnabledChannelsAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preferences = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == userId && 
                   np.NotificationType == notificationType && 
                   np.IsEnabled)
            .Select(np => np.Channel)
            .ToListAsync(cancellationToken);

        return preferences;
    }
}

