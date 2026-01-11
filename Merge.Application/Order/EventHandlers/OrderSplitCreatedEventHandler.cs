using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Split Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderSplitCreatedEventHandler : INotificationHandler<OrderSplitCreatedEvent>
{
    private readonly ILogger<OrderSplitCreatedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderSplitCreatedEventHandler(
        ILogger<OrderSplitCreatedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderSplitCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
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
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderSplitCreatedEvent. OrderSplitId: {OrderSplitId}, OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                notification.OrderSplitId, notification.OriginalOrderId, notification.SplitOrderId);
            throw;
        }
    }
}
