using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Logistics.EventHandlers;


public class PickPackDetailsUpdatedEventHandler(
    ILogger<PickPackDetailsUpdatedEventHandler> logger) : INotificationHandler<PickPackDetailsUpdatedEvent>
{

    public async Task Handle(PickPackDetailsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "PickPack details updated event received. PickPackId: {PickPackId}, OrderId: {OrderId}",
            notification.PickPackId, notification.OrderId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (pick-pack details cache)
            // - Analytics tracking (pick-pack update metrics)
            // - Notification gönderimi (warehouse staff'a pick-pack güncelleme bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling PickPackDetailsUpdatedEvent. PickPackId: {PickPackId}, OrderId: {OrderId}",
                notification.PickPackId, notification.OrderId);
            throw;
        }
    }
}
