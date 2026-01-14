using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Shipping Details Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class ShippingDetailsUpdatedEventHandler(
    ILogger<ShippingDetailsUpdatedEventHandler> logger) : INotificationHandler<ShippingDetailsUpdatedEvent>
{

    public async Task Handle(ShippingDetailsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ShippingDetailsUpdatedEvent. ShippingId: {ShippingId}, OrderId: {OrderId}, UpdateType: {UpdateType}",
                notification.ShippingId, notification.OrderId, notification.UpdateType);
            throw;
        }
    }
}
