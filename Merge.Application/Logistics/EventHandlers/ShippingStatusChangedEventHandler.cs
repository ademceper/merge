using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ShippingStatusChangedEventHandler : INotificationHandler<ShippingStatusChangedEvent>
{
    private readonly ILogger<ShippingStatusChangedEventHandler> _logger;

    public ShippingStatusChangedEventHandler(ILogger<ShippingStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ShippingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Shipping status changed event received. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.ShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya kargo durumu değişti bildirimi)
            // - Email notification (status-specific emails: shipped, delivered, etc.)
            // - Analytics tracking (shipping status transition metrics)
            // - External shipping provider webhook (status update to provider)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ShippingStatusChangedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                notification.ShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);
            throw;
        }
    }
}
