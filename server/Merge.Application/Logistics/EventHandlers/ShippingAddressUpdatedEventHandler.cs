using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressUpdatedEventHandler(
    ILogger<ShippingAddressUpdatedEventHandler> logger) : INotificationHandler<ShippingAddressUpdatedEvent>
{

    public async Task Handle(ShippingAddressUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress updated event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
            notification.ShippingAddressId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user shipping addresses cache)
            // - Analytics tracking (address update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingAddressUpdatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
