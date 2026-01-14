using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Split Cancelled Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderSplitCancelledEventHandler : INotificationHandler<OrderSplitCancelledEvent>
{
    private readonly ILogger<OrderSplitCancelledEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderSplitCancelledEventHandler(
        ILogger<OrderSplitCancelledEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderSplitCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order split cancelled event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService != null)
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
            
            // ✅ FIX: CS1998 - Async method'da await yok, Task.CompletedTask döndür
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderSplitCancelledEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
