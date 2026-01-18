using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;


public class OrderSplitCancelledEventHandler(ILogger<OrderSplitCancelledEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderSplitCancelledEvent>
{
    public async Task Handle(OrderSplitCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order split cancelled event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService is not null)
            // {
            //     await _notificationService.CreateNotificationAsync(
            //         userId,
            //         "Sipariş Bölünmesi İptal Edildi",
            //         $"Sipariş bölünmesi iptal edildi. Orijinal Sipariş No: {notification.OriginalOrderId}",
            //         "OrderSplit",
            //         cancellationToken);
            // }

            // Analytics tracking
            // await _analyticsService.TrackOrderSplitCancelledAsync(notification, cancellationToken);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderSplitCancelledEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
