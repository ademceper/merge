using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamUpdatedEventHandler(
    ILogger<LiveStreamUpdatedEventHandler> logger) : INotificationHandler<LiveStreamUpdatedEvent>
{
    public async Task Handle(LiveStreamUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream updated event received. StreamId: {StreamId}, SellerId: {SellerId}, Title: {Title}, UpdatedAt: {UpdatedAt}",
            notification.StreamId, notification.SellerId, notification.Title, notification.UpdatedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamUpdatedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
