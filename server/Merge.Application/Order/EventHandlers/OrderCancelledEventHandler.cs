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


public class OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order cancelled event received. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            notification.OrderId, notification.UserId, notification.Reason ?? "N/A");

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "Siparişiniz İptal Edildi",
                    $"Siparişiniz iptal edildi. Sipariş No: {notification.OrderId}. Sebep: {notification.Reason ?? "Belirtilmedi"}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderCancelledAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderCancelledEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
