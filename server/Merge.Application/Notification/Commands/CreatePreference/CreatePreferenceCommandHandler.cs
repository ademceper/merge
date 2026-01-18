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


public class CreatePreferenceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePreferenceCommandHandler> logger) : IRequestHandler<CreatePreferenceCommand, NotificationPreferenceDto>
{

    public async Task<NotificationPreferenceDto> Handle(CreatePreferenceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notification preference oluşturuluyor. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.Dto.NotificationType, request.Dto.Channel);

        var existing = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.Dto.NotificationType && 
                                      np.Channel == request.Dto.Channel, cancellationToken);

        NotificationPreference preference;
        if (existing is not null)
        {
            existing.Update(
                request.Dto.IsEnabled,
                request.Dto.CustomSettings is not null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : null);
            preference = existing;
        }
        else
        {
            preference = NotificationPreference.Create(
                request.UserId,
                request.Dto.NotificationType,
                request.Dto.Channel,
                request.Dto.IsEnabled,
                request.Dto.CustomSettings is not null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : null);

            await context.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdPreference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.Dto.NotificationType && 
                                      np.Channel == request.Dto.Channel, cancellationToken);

        if (createdPreference is null)
        {
            throw new Application.Exceptions.BusinessException("Tercih oluşturulamadı.");
        }

        logger.LogInformation(
            "Notification preference oluşturuldu. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.Dto.NotificationType, request.Dto.Channel);

        return mapper.Map<NotificationPreferenceDto>(createdPreference);
    }
}
