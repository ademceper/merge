using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// ShippingAddress Unset As Default Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ShippingAddressUnsetAsDefaultEventHandler : INotificationHandler<ShippingAddressUnsetAsDefaultEvent>
{
    private readonly ILogger<ShippingAddressUnsetAsDefaultEventHandler> _logger;

    public ShippingAddressUnsetAsDefaultEventHandler(ILogger<ShippingAddressUnsetAsDefaultEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ShippingAddressUnsetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ShippingAddressUnsetAsDefaultEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
