using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamProductShowcasedEventHandler(
    ILogger<LiveStreamProductShowcasedEventHandler> logger) : INotificationHandler<LiveStreamProductShowcasedEvent>
{
    public async Task Handle(LiveStreamProductShowcasedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream product showcased event received. StreamId: {StreamId}, ProductId: {ProductId}, ShowcasedAt: {ShowcasedAt}",
            notification.StreamId, notification.ProductId, notification.ShowcasedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamProductShowcasedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
