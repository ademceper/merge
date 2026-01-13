using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Status Changed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class ShippingStatusChangedEventHandler(
    ILogger<ShippingStatusChangedEventHandler> logger) : INotificationHandler<ShippingStatusChangedEvent>
{

    public async Task Handle(ShippingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling ShippingStatusChangedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                notification.ShippingId, notification.OrderId, notification.OldStatus, notification.NewStatus);
            throw;
        }
    }
}
