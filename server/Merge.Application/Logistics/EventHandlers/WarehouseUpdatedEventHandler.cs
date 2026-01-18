using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class WarehouseUpdatedEventHandler(
    ILogger<WarehouseUpdatedEventHandler> logger) : INotificationHandler<WarehouseUpdatedEvent>
{

    public async Task Handle(WarehouseUpdatedEvent notification, CancellationToken cancellationToken)
    {
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
            logger.LogError(ex,
                "Error handling WarehouseUpdatedEvent. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
                notification.WarehouseId, notification.Name, notification.Code);
            throw;
        }
    }
}
