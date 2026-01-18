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


public class OrderDeliveredEventHandler(ILogger<OrderDeliveredEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderDeliveredEvent>
{
    public async Task Handle(OrderDeliveredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order delivered event received. OrderId: {OrderId}, UserId: {UserId}, DeliveredDate: {DeliveredDate}",
            notification.OrderId, notification.UserId, notification.DeliveredDate);

        try
        {
            // Email bildirimi gönder
            if (notificationService is not null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.Order,
                    "Siparişiniz Teslim Edildi",
                    $"Siparişiniz teslim edildi. Sipariş No: {notification.OrderId}",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderDeliveredAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderDeliveredEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
