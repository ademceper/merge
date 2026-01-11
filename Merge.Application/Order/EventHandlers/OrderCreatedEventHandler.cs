using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Common.DomainEvents;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Interfaces;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Order.EventHandlers;

/// <summary>
/// Order Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    private readonly INotificationService? _notificationService;
    private readonly IDbContext _context;

    public OrderCreatedEventHandler(
        ILogger<OrderCreatedEventHandler> logger,
        IDbContext context,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Order entity'sinden gerçek TotalAmount'u al
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

        var totalAmount = order?.TotalAmount ?? notification.TotalAmount;

        _logger.LogInformation(
            "Order created event received. OrderId: {OrderId}, UserId: {UserId}, TotalAmount: {TotalAmount}",
            notification.OrderId, notification.UserId, totalAmount);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notification.UserId,
                    "Sipariş Oluşturuldu",
                    $"Siparişiniz başarıyla oluşturuldu. Sipariş No: {notification.OrderId}",
                    "Order",
                    cancellationToken);
            }

            // Analytics tracking
            // await _analyticsService.TrackOrderCreatedAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling OrderCreatedEvent. OrderId: {OrderId}, UserId: {UserId}",
                notification.OrderId, notification.UserId);
            throw;
        }
    }
}
