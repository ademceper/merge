using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class PickPackStatusChangedEventHandler(
    ILogger<PickPackStatusChangedEventHandler> logger) : INotificationHandler<PickPackStatusChangedEvent>
{

    public async Task Handle(PickPackStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PickPack status changed event received. PickPackId: {PickPackId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            notification.PickPackId, notification.OrderId, notification.OldStatus, notification.NewStatus);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Notification gönderimi (warehouse staff'a status değişikliği bildirimi)
            // - Analytics tracking (pick-pack status transition metrics)
            // - Cache invalidation (warehouse pick-pack stats cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling PickPackStatusChangedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                notification.PickPackId, notification.OrderId, notification.OldStatus, notification.NewStatus);
            throw;
        }
    }
}
