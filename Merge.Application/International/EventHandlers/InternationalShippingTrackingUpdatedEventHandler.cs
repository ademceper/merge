using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// InternationalShipping Tracking Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InternationalShippingTrackingUpdatedEventHandler : INotificationHandler<InternationalShippingTrackingUpdatedEvent>
{
    private readonly ILogger<InternationalShippingTrackingUpdatedEventHandler> _logger;

    public InternationalShippingTrackingUpdatedEventHandler(ILogger<InternationalShippingTrackingUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InternationalShippingTrackingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "International shipping tracking updated event received. ShippingId: {ShippingId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
            notification.InternationalShippingId, notification.OrderId, notification.TrackingNumber);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (kullanıcıya takip numarası bildirimi)
            // - External shipping provider tracking sync
            // - Analytics tracking (tracking update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InternationalShippingTrackingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
