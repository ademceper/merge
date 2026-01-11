using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using System.Text.Json;

namespace Merge.Application.Notification.Commands.BulkUpdatePreferences;

/// <summary>
/// Bulk Update Preferences Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class BulkUpdatePreferencesCommandHandler : IRequestHandler<BulkUpdatePreferencesCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkUpdatePreferencesCommandHandler> _logger;

    public BulkUpdatePreferencesCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<BulkUpdatePreferencesCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(BulkUpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.Preferences == null || request.Dto.Preferences.Count == 0)
        {
            return true;
        }

        // ✅ PERFORMANCE: Batch load existing preferences (N+1 fix)
        var notificationTypes = request.Dto.Preferences.Select(p => p.NotificationType).Distinct().ToList();
        var channels = request.Dto.Preferences.Select(p => p.Channel).Distinct().ToList();
        
        var existingPreferencesList = await _context.Set<NotificationPreference>()
            .Where(np => np.UserId == request.UserId && 
                        notificationTypes.Contains(np.NotificationType) && 
                        channels.Contains(np.Channel))
            .ToListAsync(cancellationToken);
        
        var existingPreferences = existingPreferencesList
            .ToDictionary(np => new { np.NotificationType, np.Channel }, np => np);

        var preferencesToAdd = new List<NotificationPreference>();

        foreach (var prefDto in request.Dto.Preferences)
        {
            var key = new { prefDto.NotificationType, prefDto.Channel };
            if (existingPreferences.TryGetValue(key, out var existing))
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                existing.Update(
                    prefDto.IsEnabled,
                    prefDto.CustomSettings != null ? JsonSerializer.Serialize(prefDto.CustomSettings) : null);
            }
            else
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                var preference = NotificationPreference.Create(
                    request.UserId,
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
            await _context.Set<NotificationPreference>().AddRangeAsync(preferencesToAdd, cancellationToken);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Toplu notification preference güncellendi. UserId: {UserId}, Count: {Count}",
            request.UserId, request.Dto.Preferences.Count);

        return true;
    }
}
