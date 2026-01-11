using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Cancelled Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderCancelledEventHandler(
        ILogger<OrderCancelledEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order cancelled event received. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            notification.OrderId, notification.UserId, notification.Reason ?? "N/A");

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notification.UserId,
                    "Siparişiniz İptal Edildi",
                    $"Siparişiniz iptal edildi. Sipariş No: {notification.OrderId}. Sebep: {notification.Reason ?? "Belirtilmedi"}",
                    "Order",
                    cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderCancelledAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderCancelledEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
