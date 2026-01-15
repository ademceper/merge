using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamCancelledEventHandler(
    ILogger<LiveStreamCancelledEventHandler> logger) : INotificationHandler<LiveStreamCancelledEvent>
{
    public async Task Handle(LiveStreamCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream cancelled event received. StreamId: {StreamId}, SellerId: {SellerId}, CancelledAt: {CancelledAt}",
            notification.StreamId, notification.SellerId, notification.CancelledAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamCancelledEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
