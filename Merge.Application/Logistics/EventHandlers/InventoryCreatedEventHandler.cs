using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Inventory Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
/// </summary>
public class InventoryCreatedEventHandler(
    ILogger<InventoryCreatedEventHandler> logger) : INotificationHandler<InventoryCreatedEvent>
{

    public async Task Handle(InventoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling InventoryCreatedEvent. InventoryId: {InventoryId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                notification.InventoryId, notification.ProductId, notification.WarehouseId);
            throw;
        }
    }
}
