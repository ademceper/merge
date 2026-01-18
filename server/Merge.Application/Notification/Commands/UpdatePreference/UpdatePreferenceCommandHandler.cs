using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.UpdatePreference;


public class UpdatePreferenceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdatePreferenceCommandHandler> logger) : IRequestHandler<UpdatePreferenceCommand, NotificationPreferenceDto>
{

    public async Task<NotificationPreferenceDto> Handle(UpdatePreferenceCommand request, CancellationToken cancellationToken)
    {
        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        if (preference is null)
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        preference.Update(
            request.Dto.IsEnabled ?? preference.IsEnabled,
            request.Dto.CustomSettings is not null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : preference.CustomSettings);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification preference güncellendi. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.NotificationType, request.Channel);

        return mapper.Map<NotificationPreferenceDto>(preference);
    }
}
