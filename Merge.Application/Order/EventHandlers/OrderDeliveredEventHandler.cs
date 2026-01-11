using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Delivered Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderDeliveredEventHandler : INotificationHandler<OrderDeliveredEvent>
{
    private readonly ILogger<OrderDeliveredEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public OrderDeliveredEventHandler(
        ILogger<OrderDeliveredEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderDeliveredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order delivered event received. OrderId: {OrderId}, UserId: {UserId}, DeliveredDate: {DeliveredDate}",
            notification.OrderId, notification.UserId, notification.DeliveredDate);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notification.UserId,
                    "Siparişiniz Teslim Edildi",
                    $"Siparişiniz teslim edildi. Sipariş No: {notification.OrderId}",
                    "Order",
                    cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderDeliveredAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderDeliveredEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
