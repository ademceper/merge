using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;

/// <summary>
/// Warehouse Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class WarehouseDeactivatedEventHandler : INotificationHandler<WarehouseDeactivatedEvent>
{
    private readonly ILogger<WarehouseDeactivatedEventHandler> _logger;

    public WarehouseDeactivatedEventHandler(ILogger<WarehouseDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(WarehouseDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Warehouse deactivated event received. WarehouseId: {WarehouseId}",
            notification.WarehouseId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active warehouses cache)
            // - Analytics tracking (warehouse deactivation metrics)
            // - Notification gönderimi (warehouse manager'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling WarehouseDeactivatedEvent. WarehouseId: {WarehouseId}",
                notification.WarehouseId);
            throw;
        }
    }
}
