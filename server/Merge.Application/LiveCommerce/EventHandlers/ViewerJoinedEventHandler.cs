using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class ViewerJoinedEventHandler(
    ILogger<ViewerJoinedEventHandler> logger) : INotificationHandler<ViewerJoinedEvent>
{
    public async Task Handle(ViewerJoinedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Viewer joined event received. StreamId: {StreamId}, ViewerId: {ViewerId}, UserId: {UserId}, GuestId: {GuestId}, JoinedAt: {JoinedAt}",
            notification.StreamId, notification.ViewerId, notification.UserId, notification.GuestId, notification.JoinedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ViewerJoinedEvent. StreamId: {StreamId}, ViewerId: {ViewerId}",
                notification.StreamId, notification.ViewerId);
            throw;
        }
    }
}
