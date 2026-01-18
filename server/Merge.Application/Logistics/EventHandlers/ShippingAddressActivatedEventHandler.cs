using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressActivatedEventHandler(
    ILogger<ShippingAddressActivatedEventHandler> logger) : INotificationHandler<ShippingAddressActivatedEvent>
{

    public async Task Handle(ShippingAddressActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress activated event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
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
                "Error handling ShippingAddressActivatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
