using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamProductUnhighlightedEventHandler(
    ILogger<LiveStreamProductUnhighlightedEventHandler> logger) : INotificationHandler<LiveStreamProductUnhighlightedEvent>
{
    public async Task Handle(LiveStreamProductUnhighlightedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream product unhighlighted event received. StreamId: {StreamId}, ProductId: {ProductId}, UnhighlightedAt: {UnhighlightedAt}",
            notification.StreamId, notification.ProductId, notification.UnhighlightedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamProductUnhighlightedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
