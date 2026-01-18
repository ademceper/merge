using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class WarehouseActivatedEventHandler(
    ILogger<WarehouseActivatedEventHandler> logger) : INotificationHandler<WarehouseActivatedEvent>
{

    public async Task Handle(WarehouseActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling WarehouseActivatedEvent. WarehouseId: {WarehouseId}",
                notification.WarehouseId);
            throw;
        }
    }
}
