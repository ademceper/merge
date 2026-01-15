using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamOrderDeletedEventHandler(
    ILogger<LiveStreamOrderDeletedEventHandler> logger) : INotificationHandler<LiveStreamOrderDeletedEvent>
{
    public async Task Handle(LiveStreamOrderDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream order deleted event received. StreamId: {StreamId}, OrderId: {OrderId}, OrderAmount: {OrderAmount}, DeletedAt: {DeletedAt}",
            notification.StreamId, notification.OrderId, notification.OrderAmount, notification.DeletedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamOrderDeletedEvent. StreamId: {StreamId}, OrderId: {OrderId}",
                notification.StreamId, notification.OrderId);
            throw;
        }
    }
}
