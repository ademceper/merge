using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class PickPackCreatedEventHandler(
    ILogger<PickPackCreatedEventHandler> logger) : INotificationHandler<PickPackCreatedEvent>
{

    public async Task Handle(PickPackCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PickPack created event received. PickPackId: {PickPackId}, OrderId: {OrderId}, WarehouseId: {WarehouseId}, PackNumber: {PackNumber}",
            notification.PickPackId, notification.OrderId, notification.WarehouseId, notification.PackNumber);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (warehouse staff'a yeni pick-pack bildirimi)
            // - Analytics tracking (pick-pack creation metrics)
            // - Cache invalidation (warehouse pick-pack stats cache)
            // - External system integration (WMS, ERP)
            // - Email notification (warehouse manager'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling PickPackCreatedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}, PackNumber: {PackNumber}",
                notification.PickPackId, notification.OrderId, notification.PackNumber);
            throw;
        }
    }
}
