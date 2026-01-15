using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamOrderRestoredEventHandler(
    ILogger<LiveStreamOrderRestoredEventHandler> logger) : INotificationHandler<LiveStreamOrderRestoredEvent>
{
    public async Task Handle(LiveStreamOrderRestoredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream order restored event received. StreamId: {StreamId}, OrderId: {OrderId}, OrderAmount: {OrderAmount}, RestoredAt: {RestoredAt}",
            notification.StreamId, notification.OrderId, notification.OrderAmount, notification.RestoredAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamOrderRestoredEvent. StreamId: {StreamId}, OrderId: {OrderId}",
                notification.StreamId, notification.OrderId);
            throw;
        }
    }
}
