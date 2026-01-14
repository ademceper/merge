using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// ShippingAddress Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class ShippingAddressUpdatedEventHandler(
    ILogger<ShippingAddressUpdatedEventHandler> logger) : INotificationHandler<ShippingAddressUpdatedEvent>
{

    public async Task Handle(ShippingAddressUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ShippingAddressUpdatedEvent. ShippingAddressId: {ShippingAddressId}, UserId: {UserId}",
                notification.ShippingAddressId, notification.UserId);
            throw;
        }
    }
}
