using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingDetailsUpdatedEventHandler(
    ILogger<ShippingDetailsUpdatedEventHandler> logger) : INotificationHandler<ShippingDetailsUpdatedEvent>
{

    public async Task Handle(ShippingDetailsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Shipping details updated event received. ShippingId: {ShippingId}, OrderId: {OrderId}, UpdateType: {UpdateType}",
            notification.ShippingId, notification.OrderId, notification.UpdateType);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (shipping details cache)
            // - Analytics tracking (shipping update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingDetailsUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, UpdateType: {UpdateType}",
                notification.ShippingId, notification.OrderId, notification.UpdateType);
            throw;
        }
    }
}
