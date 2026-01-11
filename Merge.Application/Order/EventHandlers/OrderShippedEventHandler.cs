using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Shipped Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderShippedEventHandler : INotificationHandler<OrderShippedEvent>
{
    private readonly ILogger<OrderShippedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderShippedEventHandler(
        ILogger<OrderShippedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order shipped event received. OrderId: {OrderId}, UserId: {UserId}, ShippedDate: {ShippedDate}",
            notification.OrderId, notification.UserId, notification.ShippedDate);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notification.UserId,
                    "Siparişiniz Kargoya Verildi",
                    $"Siparişiniz kargoya verildi. Sipariş No: {notification.OrderId}",
                    "Order",
                    cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderShippedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderShippedEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
