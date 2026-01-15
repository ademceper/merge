using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamCreatedEventHandler(
    ILogger<LiveStreamCreatedEventHandler> logger) : INotificationHandler<LiveStreamCreatedEvent>
{
    public async Task Handle(LiveStreamCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream created event received. StreamId: {StreamId}, SellerId: {SellerId}, Title: {Title}, ScheduledStartTime: {ScheduledStartTime}",
            notification.StreamId, notification.SellerId, notification.Title, notification.ScheduledStartTime);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamCreatedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
