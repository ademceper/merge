using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class SizeGuideDeletedEventHandler(
    ILogger<SizeGuideDeletedEventHandler> logger) : INotificationHandler<SizeGuideDeletedEvent>
{

    public async Task Handle(SizeGuideDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Size guide deleted event received. SizeGuideId: {SizeGuideId}, Name: {Name}",
            notification.SizeGuideId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (size guides cache)
            // - Analytics tracking (size guide deletion metrics)
            // - External system integration
            // - Cascade delete handling (size guide entries, product size guides)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SizeGuideDeletedEvent. SizeGuideId: {SizeGuideId}, Name: {Name}",
                notification.SizeGuideId, notification.Name);
            throw;
        }
    }
}
