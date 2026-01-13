using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// StockMovement Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class StockMovementCreatedEventHandler : INotificationHandler<StockMovementCreatedEvent>
{
    private readonly ILogger<StockMovementCreatedEventHandler> _logger;

    public StockMovementCreatedEventHandler(ILogger<StockMovementCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(StockMovementCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "StockMovement created event received. StockMovementId: {StockMovementId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}, MovementType: {MovementType}, Quantity: {Quantity}, QuantityBefore: {QuantityBefore}, QuantityAfter: {QuantityAfter}",
            notification.StockMovementId, notification.ProductId, notification.WarehouseId, notification.MovementType, notification.Quantity, notification.QuantityBefore, notification.QuantityAfter);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Low stock alert kontrolü (QuantityAfter < MinimumStockLevel)
            // - Analytics tracking (stock movement metrics)
            // - Cache invalidation (product stock cache, warehouse inventory cache)
            // - External system integration (WMS, ERP, Inventory system)
            // - Audit log (stock movement history)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling StockMovementCreatedEvent. StockMovementId: {StockMovementId}, ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                notification.StockMovementId, notification.ProductId, notification.WarehouseId);
            throw;
        }
    }
}
