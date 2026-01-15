using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamResumedEventHandler(
    ILogger<LiveStreamResumedEventHandler> logger) : INotificationHandler<LiveStreamResumedEvent>
{
    public async Task Handle(LiveStreamResumedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream resumed event received. StreamId: {StreamId}, SellerId: {SellerId}, ResumedAt: {ResumedAt}",
            notification.StreamId, notification.SellerId, notification.ResumedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamResumedEvent. StreamId: {StreamId}, SellerId: {SellerId}",
                notification.StreamId, notification.SellerId);
            throw;
        }
    }
}
