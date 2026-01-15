using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamRestoredEventHandler(
    ILogger<LiveStreamRestoredEventHandler> logger) : INotificationHandler<LiveStreamRestoredEvent>
{
    public async Task Handle(LiveStreamRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream restored event received. StreamId: {StreamId}, SellerId: {SellerId}, Title: {Title}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.SellerId, notification.Title, notification.RestoredAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamRestoredEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
