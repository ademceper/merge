using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.LiveCommerce.EventHandlers;

public class LiveStreamProductAddedEventHandler(
    ILogger<LiveStreamProductAddedEventHandler> logger) : INotificationHandler<LiveStreamProductAddedEvent>
{
    public async Task Handle(LiveStreamProductAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Live stream product added event received. StreamId: {StreamId}, ProductId: {ProductId}, SpecialPrice: {SpecialPrice}",
            notification.StreamId, notification.ProductId, notification.SpecialPrice);

        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LiveStreamProductAddedEvent. StreamId: {StreamId}, ProductId: {ProductId}",
                notification.StreamId, notification.ProductId);
            throw;
        }
    }
}
