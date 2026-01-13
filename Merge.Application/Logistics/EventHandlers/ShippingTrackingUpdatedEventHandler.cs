using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Tracking Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ShippingTrackingUpdatedEventHandler : INotificationHandler<ShippingTrackingUpdatedEvent>
{
    private readonly ILogger<ShippingTrackingUpdatedEventHandler> _logger;

    public ShippingTrackingUpdatedEventHandler(ILogger<ShippingTrackingUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ShippingTrackingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            _logger.LogError(ex,
                "Error handling ShippingTrackingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
                notification.ShippingId, notification.OrderId, notification.TrackingNumber);
            throw;
        }
    }
}
