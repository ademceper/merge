using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;


public class OrderSplitCompletedEventHandler(ILogger<OrderSplitCompletedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderSplitCompletedEvent>
{
    public async Task Handle(OrderSplitCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order split completed event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService is not null)
            // {
            //     await _notificationService.CreateNotificationAsync(
            //         userId,
            //         "Sipariş Bölünmesi Tamamlandı",
            //         $"Sipariş bölünmesi tamamlandı. Orijinal Sipariş No: {notification.OriginalOrderId}, Yeni Sipariş No: {notification.SplitOrderId}",
            //         "OrderSplit",
            //         cancellationToken);
            // }

            // Analytics tracking
            // await _analyticsService.TrackOrderSplitCompletedAsync(notification, cancellationToken);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling OrderSplitCompletedEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
