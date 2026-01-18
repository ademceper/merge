using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class WarehouseDeactivatedEventHandler(
    ILogger<WarehouseDeactivatedEventHandler> logger) : INotificationHandler<WarehouseDeactivatedEvent>
{

    public async Task Handle(WarehouseDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling WarehouseDeactivatedEvent. WarehouseId: {WarehouseId}",
                notification.WarehouseId);
            throw;
        }
    }
}
