using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamViewerDeletedEventHandler(
    ILogger<LiveStreamViewerDeletedEventHandler> logger) : INotificationHandler<LiveStreamViewerDeletedEvent>
{
    public async Task Handle(LiveStreamViewerDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream viewer deleted event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.DeletedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamViewerDeletedEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
