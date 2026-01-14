using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Warehouse Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class WarehouseUpdatedEventHandler(
    ILogger<WarehouseUpdatedEventHandler> logger) : INotificationHandler<WarehouseUpdatedEvent>
{

    public async Task Handle(WarehouseUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Warehouse updated event received. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
            notification.WarehouseId, notification.Name, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (warehouses cache)
            // - Analytics tracking (warehouse update metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling WarehouseUpdatedEvent. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
                notification.WarehouseId, notification.Name, notification.Code);
            throw;
        }
    }
}
