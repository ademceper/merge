using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class WarehouseCreatedEventHandler(
    ILogger<WarehouseCreatedEventHandler> logger) : INotificationHandler<WarehouseCreatedEvent>
{

    public async Task Handle(WarehouseCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Warehouse created event received. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
            notification.WarehouseId, notification.Name, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (warehouses cache)
            // - Analytics tracking (warehouse creation metrics)
            // - External system integration (WMS, ERP)
            // - Notification gönderimi (admin'lere yeni depo oluşturuldu bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling WarehouseCreatedEvent. WarehouseId: {WarehouseId}, Name: {Name}, Code: {Code}",
                notification.WarehouseId, notification.Name, notification.Code);
            throw;
        }
    }
}
