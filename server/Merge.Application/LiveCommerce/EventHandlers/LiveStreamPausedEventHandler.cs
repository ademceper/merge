using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamPausedEventHandler(
    ILogger<LiveStreamPausedEventHandler> logger) : INotificationHandler<LiveStreamPausedEvent>
{
    public async Task Handle(LiveStreamPausedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream paused event received. StreamId: {StreamId}, SellerId: {SellerId}, PausedAt: {PausedAt}",
            notification.StreamId, notification.SellerId, notification.PausedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamPausedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
