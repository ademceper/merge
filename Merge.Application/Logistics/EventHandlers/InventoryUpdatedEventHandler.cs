using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Inventory Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InventoryUpdatedEventHandler : INotificationHandler<InventoryUpdatedEvent>
{
    private readonly ILogger<InventoryUpdatedEventHandler> _logger;

    public InventoryUpdatedEventHandler(ILogger<InventoryUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InventoryUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Inventory updated event received. InventoryId: {InventoryId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            notification.InventoryId, notification.ProductId, notification.WarehouseId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Low stock alert kontrolü (Quantity < MinimumStockLevel)
            // - Analytics tracking (inventory update metrics)
            // - Cache invalidation (product stock cache, warehouse inventory cache)
            // - External system integration (WMS, ERP, Inventory system)
            // - Audit log (inventory update history)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InventoryUpdatedEvent. InventoryId: {InventoryId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                notification.InventoryId, notification.ProductId, notification.WarehouseId);
            throw;
        }
    }
}
