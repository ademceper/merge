using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamOrderCreatedEventHandler(
    ILogger<LiveStreamOrderCreatedEventHandler> logger) : INotificationHandler<LiveStreamOrderCreatedEvent>
{
    public async Task Handle(LiveStreamOrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream order created event received. StreamId: {StreamId}, OrderId: {OrderId}, ProductId: {ProductId}, OrderAmount: {OrderAmount}, CreatedAt: {CreatedAt}",
            notification.StreamId, notification.OrderId, notification.ProductId, notification.OrderAmount, notification.CreatedAt);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamOrderCreatedEvent. StreamId: {StreamId}, OrderId: {OrderId}",
                notification.StreamId, notification.OrderId);
            throw;
        }
    }
}
