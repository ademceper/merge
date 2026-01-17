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

/// <summary>
/// Create Notification Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreateNotificationCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateNotificationCommandHandler> logger) : IRequestHandler<CreateNotificationCommand, NotificationDto>
{

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var notification = NotificationEntity.Create(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.Link,
            request.Data);

        await context.Set<NotificationEntity>().AddAsync(notification, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification oluşturuldu. NotificationId: {NotificationId}, UserId: {UserId}, Type: {Type}",
            notification.Id, request.UserId, request.Type);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationDto>(notification);
    }
}
