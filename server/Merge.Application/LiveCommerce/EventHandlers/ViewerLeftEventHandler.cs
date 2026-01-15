using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class ViewerLeftEventHandler(
    ILogger<ViewerLeftEventHandler> logger) : INotificationHandler<ViewerLeftEvent>
{
    public async Task Handle(ViewerLeftEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Viewer left event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, LeftAt: {LeftAt}, WatchDurationInSeconds: {WatchDurationInSeconds}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.LeftAt, notification.WatchDurationInSeconds);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ViewerLeftEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
