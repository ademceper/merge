using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Inventory Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class InventoryCreatedEventHandler : INotificationHandler<InventoryCreatedEvent>
{
    private readonly ILogger<InventoryCreatedEventHandler> _logger;

    public InventoryCreatedEventHandler(ILogger<InventoryCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InventoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Inventory created event received. InventoryId: {InventoryId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}",
            notification.InventoryId, notification.ProductId, notification.WarehouseId, notification.Quantity);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Low stock alert kontrolü (Quantity < MinimumStockLevel)
            // - Analytics tracking (inventory creation metrics)
            // - Cache invalidation (product stock cache, warehouse inventory cache)
            // - External system integration (WMS, ERP, Inventory system)
            // - Email notification (warehouse manager'a yeni inventory bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling InventoryCreatedEvent. InventoryId: {InventoryId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                notification.InventoryId, notification.ProductId, notification.WarehouseId);
            throw;
        }
    }
}
