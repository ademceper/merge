using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamProductDeletedEventHandler(
    ILogger<LiveStreamProductDeletedEventHandler> logger) : INotificationHandler<LiveStreamProductDeletedEvent>
{
    public async Task Handle(LiveStreamProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream product deleted event received. StreamId: {StreamId}, ProductId: {ProductId}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.ProductId, notification.DeletedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamProductDeletedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
