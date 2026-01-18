using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressSetAsDefaultEventHandler(
    ILogger<ShippingAddressSetAsDefaultEventHandler> logger) : INotificationHandler<ShippingAddressSetAsDefaultEvent>
{

    public async Task Handle(ShippingAddressSetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress set as default event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
            notification.ShippingAddressId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user default shipping address cache)
            // - Analytics tracking (default address change metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingAddressSetAsDefaultEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
