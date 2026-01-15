using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class InternationalShippingTrackingUpdatedEventHandler(ILogger<InternationalShippingTrackingUpdatedEventHandler> logger) : INotificationHandler<InternationalShippingTrackingUpdatedEvent>
{
    public async Task Handle(InternationalShippingTrackingUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling InternationalShippingTrackingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}",
                notification.InternationalShippingId, notification.OrderId);
            throw;
        }
    }
}
