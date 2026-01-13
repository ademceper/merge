using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Warehouse Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class WarehouseActivatedEventHandler : INotificationHandler<WarehouseActivatedEvent>
{
    private readonly ILogger<WarehouseActivatedEventHandler> _logger;

    public WarehouseActivatedEventHandler(ILogger<WarehouseActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(WarehouseActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Warehouse activated event received. WarehouseId: {WarehouseId}",
            notification.WarehouseId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active warehouses cache)
            // - Analytics tracking (warehouse activation metrics)
            // - Notification gönderimi (warehouse manager'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling WarehouseActivatedEvent. WarehouseId: {WarehouseId}",
                notification.WarehouseId);
            throw;
        }
    }
}
