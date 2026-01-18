using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.CreateNotification;


public class CreateNotificationCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateNotificationCommandHandler> logger) : IRequestHandler<CreateNotificationCommand, NotificationDto>
{

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = NotificationEntity.Create(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.Link,
            request.Data);

        await context.Set<NotificationEntity>().AddAsync(notification, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification oluşturuldu. NotificationId: {NotificationId}, UserId: {UserId}, Type: {Type}",
            notification.Id, request.UserId, request.Type);

        return mapper.Map<NotificationDto>(notification);
    }
}
