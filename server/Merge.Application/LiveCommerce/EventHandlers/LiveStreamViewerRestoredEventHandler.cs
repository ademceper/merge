using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamViewerRestoredEventHandler(
    ILogger<LiveStreamViewerRestoredEventHandler> logger) : INotificationHandler<LiveStreamViewerRestoredEvent>
{
    public async Task Handle(LiveStreamViewerRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream viewer restored event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.RestoredAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamViewerRestoredEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
