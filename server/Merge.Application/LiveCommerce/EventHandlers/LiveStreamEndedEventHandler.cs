using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamEndedEventHandler(
    ILogger<LiveStreamEndedEventHandler> logger) : INotificationHandler<LiveStreamEndedEvent>
{
    public async Task Handle(LiveStreamEndedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream ended event received. StreamId: {StreamId}, SellerId: {SellerId}, EndedAt: {EndedAt}, TotalViewerCount: {TotalViewerCount}, OrderCount: {OrderCount}, Revenue: {Revenue}",
            notification.StreamId, notification.SellerId, notification.EndedAt, notification.TotalViewerCount, notification.OrderCount, notification.Revenue);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamEndedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
