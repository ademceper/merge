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


public class OrderShippedEventHandler(ILogger<OrderShippedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderShippedEvent>
{
    public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order shipped event received. OrderId: {OrderId}, UserId: {UserId}, ShippedDate: {ShippedDate}",
            notification.OrderId, notification.UserId, notification.ShippedDate);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "Siparişiniz Kargoya Verildi",
                    $"Siparişiniz kargoya verildi. Sipariş No: {notification.OrderId}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderShippedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderShippedEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
