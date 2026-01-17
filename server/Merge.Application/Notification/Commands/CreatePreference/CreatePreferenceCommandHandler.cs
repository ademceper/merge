using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.CreatePreference;

/// <summary>
/// Create Preference Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreatePreferenceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePreferenceCommandHandler> logger) : IRequestHandler<CreatePreferenceCommand, NotificationPreferenceDto>
{

    public async Task<NotificationPreferenceDto> Handle(CreatePreferenceCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification preference oluşturuluyor. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.Dto.NotificationType, request.Dto.Channel);

        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var existing = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.Dto.NotificationType && 
                                      np.Channel == request.Dto.Channel, cancellationToken);

        NotificationPreference preference;
        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Update(
                request.Dto.IsEnabled,
                request.Dto.CustomSettings != null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : null);
            preference = existing;
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            preference = NotificationPreference.Create(
                request.UserId,
                request.Dto.NotificationType,
                request.Dto.Channel,
                request.Dto.IsEnabled,
                request.Dto.CustomSettings != null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : null);

            await context.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var createdPreference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.Dto.NotificationType && 
                                      np.Channel == request.Dto.Channel, cancellationToken);

        if (createdPreference == null)
        {
            throw new Application.Exceptions.BusinessException("Tercih oluşturulamadı.");
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification preference oluşturuldu. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.Dto.NotificationType, request.Dto.Channel);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationPreferenceDto>(createdPreference);
    }
}
