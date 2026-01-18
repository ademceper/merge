using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingTrackingUpdatedEventHandler(
    ILogger<ShippingTrackingUpdatedEventHandler> logger) : INotificationHandler<ShippingTrackingUpdatedEvent>
{

    public async Task Handle(ShippingTrackingUpdatedEvent notification, CancellationToken cancellationToken)
    {
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
            logger.LogError(ex,
                "Error handling ShippingTrackingUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
                notification.ShippingId, notification.OrderId, notification.TrackingNumber);
            throw;
        }
    }
}
