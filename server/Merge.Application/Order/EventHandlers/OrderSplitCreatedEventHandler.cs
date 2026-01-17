using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Split Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderSplitCreatedEventHandler(ILogger<OrderSplitCreatedEventHandler> logger, INotificationService? notificationService) : INotificationHandler<OrderSplitCreatedEvent>
{
    
    private readonly INotificationService? _notificationService;

    public async Task Handle(OrderSplitCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order split created event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}, SplitReason: {SplitReason}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId, notification.SplitReason);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService != null)
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
            
            // ✅ FIX: CS1998 - Async method'da await yok, Task.CompletedTask döndür
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling OrderSplitCreatedEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
