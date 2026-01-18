using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;


public class ReturnRequestCreatedEventHandler(ILogger<ReturnRequestCreatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<ReturnRequestCreatedEvent>
{
    public async Task Handle(ReturnRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Return request created event received. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}, RefundAmount: {RefundAmount}",
            notification.ReturnRequestId, notification.OrderId, notification.UserId, notification.RefundAmount);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "İade Talebi Oluşturuldu",
                    $"İade talebiniz başarıyla oluşturuldu. İade Talebi No: {notification.ReturnRequestId}, İade Tutarı: {notification.RefundAmount} TL",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackReturnRequestCreatedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ReturnRequestCreatedEvent. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, UserId: {UserId}",
                notification.ReturnRequestId, notification.OrderId, notification.UserId);
            throw;
        }
    }
}
