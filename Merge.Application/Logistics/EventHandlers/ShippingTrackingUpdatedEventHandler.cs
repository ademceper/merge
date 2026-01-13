using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Tracking Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class ShippingTrackingUpdatedEventHandler(
    ILogger<ShippingTrackingUpdatedEventHandler> logger) : INotificationHandler<ShippingTrackingUpdatedEvent>
{

    public async Task Handle(ShippingTrackingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Shipping tracking updated event received. ShippingId: {ShippingId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
            notification.ShippingId, notification.OrderId, notification.TrackingNumber);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya takip numarası bildirimi)
            // - Email notification (tracking number email)
            // - SMS notification (tracking number SMS)
            // - External shipping provider integration (tracking sync)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ShippingTrackingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
                notification.ShippingId, notification.OrderId, notification.TrackingNumber);
            throw;
        }
    }
}
