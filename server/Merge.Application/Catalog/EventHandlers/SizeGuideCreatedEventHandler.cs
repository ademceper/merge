using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class SizeGuideCreatedEventHandler(
    ILogger<SizeGuideCreatedEventHandler> logger) : INotificationHandler<SizeGuideCreatedEvent>
{

    public async Task Handle(SizeGuideCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Size guide created event received. SizeGuideId: {SizeGuideId}, Name: {Name}, CategoryId: {CategoryId}",
            notification.SizeGuideId, notification.Name, notification.CategoryId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (size guides cache)
            // - Analytics tracking
            // - External system integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling SizeGuideCreatedEvent. SizeGuideId: {SizeGuideId}, Name: {Name}",
                notification.SizeGuideId, notification.Name);
            throw;
        }
    }
}
