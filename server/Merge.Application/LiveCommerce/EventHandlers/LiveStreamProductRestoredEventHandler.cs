using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamProductRestoredEventHandler(
    ILogger<LiveStreamProductRestoredEventHandler> logger) : INotificationHandler<LiveStreamProductRestoredEvent>
{
    public async Task Handle(LiveStreamProductRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream product restored event received. StreamId: {StreamId}, ProductId: {ProductId}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.ProductId, notification.RestoredAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamProductRestoredEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
