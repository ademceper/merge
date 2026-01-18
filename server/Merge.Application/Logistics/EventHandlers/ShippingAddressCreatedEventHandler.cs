using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class ShippingAddressCreatedEventHandler(
    ILogger<ShippingAddressCreatedEventHandler> logger) : INotificationHandler<ShippingAddressCreatedEvent>
{

    public async Task Handle(ShippingAddressCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "ShippingAddress created event received. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}, Label: {Label}, City: {City}, Country: {Country}, IsDefault: {IsDefault}",
            notification.ShippingAddressId, notification.UserId, notification.Label, notification.City, notification.Country, notification.IsDefault);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user shipping addresses cache)
            // - Analytics tracking (address creation metrics)
            // - Address validation (external address validation service)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ShippingAddressCreatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
