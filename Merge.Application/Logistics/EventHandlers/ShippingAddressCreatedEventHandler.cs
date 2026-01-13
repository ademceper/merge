using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// ShippingAddress Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ShippingAddressCreatedEventHandler : INotificationHandler<ShippingAddressCreatedEvent>
{
    private readonly ILogger<ShippingAddressCreatedEventHandler> _logger;

    public ShippingAddressCreatedEventHandler(ILogger<ShippingAddressCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ShippingAddressCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ShippingAddressCreatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
