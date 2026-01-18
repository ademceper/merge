using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressUnsetAsDefaultEventHandler(
    ILogger<ShippingAddressUnsetAsDefaultEventHandler> logger) : INotificationHandler<ShippingAddressUnsetAsDefaultEvent>
{

    public async Task Handle(ShippingAddressUnsetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress unset as default event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
            notification.ShippingAddressId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user default shipping address cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingAddressUnsetAsDefaultEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
