using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressDeactivatedEventHandler(
    ILogger<ShippingAddressDeactivatedEventHandler> logger) : INotificationHandler<ShippingAddressDeactivatedEvent>
{

    public async Task Handle(ShippingAddressDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress deactivated event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
            notification.ShippingAddressId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user active shipping addresses cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingAddressDeactivatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
