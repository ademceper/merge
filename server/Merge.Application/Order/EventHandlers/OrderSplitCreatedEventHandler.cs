using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;


public class OrderSplitCreatedEventHandler(ILogger<OrderSplitCreatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderSplitCreatedEvent>
{
    public async Task Handle(OrderSplitCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order split created event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}, SplitReason: {SplitReason}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId, notification.SplitReason);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService is not null)
            // {
            //     await _notificationService.CreateNotificationAsync(
            //         userId,
            //         "Sipariş Bölündü",
            //         $"Siparişiniz bölündü. Orijinal Sipariş No: {notification.OriginalOrderId}, Yeni Sipariş No: {notification.SplitOrderId}",
            //         "OrderSplit",
            //         cancellationToken);
            // }

            // Analytics tracking
            // await _analyticsService.TrackOrderSplitCreatedAsync(notification, cancellationToken);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderSplitCreatedEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
