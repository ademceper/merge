using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamDeletedEventHandler(
    ILogger<LiveStreamDeletedEventHandler> logger) : INotificationHandler<LiveStreamDeletedEvent>
{
    public async Task Handle(LiveStreamDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream deleted event received. StreamId: {StreamId}, SellerId: {SellerId}, Title: {Title}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.SellerId, notification.Title, notification.DeletedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamDeletedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
