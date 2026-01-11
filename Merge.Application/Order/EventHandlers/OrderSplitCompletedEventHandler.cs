using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Split Completed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderSplitCompletedEventHandler : INotificationHandler<OrderSplitCompletedEvent>
{
    private readonly ILogger<OrderSplitCompletedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderSplitCompletedEventHandler(
        ILogger<OrderSplitCompletedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderSplitCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order split completed event received. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
            notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);

        try
        {
            // Email bildirimi gönder (order owner'a)
            // Note: UserId'yi almak için order'ı query etmek gerekebilir, şimdilik log'lanıyor
            // if (_notificationService != null)
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
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderSplitCompletedEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
