using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamStartedEventHandler(
    ILogger<LiveStreamStartedEventHandler> logger) : INotificationHandler<LiveStreamStartedEvent>
{
    public async Task Handle(LiveStreamStartedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream started event received. StreamId: {StreamId}, SellerId: {SellerId}, StartedAt: {StartedAt}",
            notification.StreamId, notification.SellerId, notification.StartedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamStartedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
