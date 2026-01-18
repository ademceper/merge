using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductComparisonDeletedEventHandler(
    ILogger<ProductComparisonDeletedEventHandler> logger) : INotificationHandler<ProductComparisonDeletedEvent>
{

    public async Task Handle(ProductComparisonDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product comparison deleted event received. ComparisonId: {ComparisonId}, UserId: {UserId}",
            notification.ComparisonId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product comparisons cache)
            // - Analytics tracking (comparison deletion metrics)
            // - External system integration
            // - Cascade delete handling (comparison items)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductComparisonDeletedEvent. ComparisonId: {ComparisonId}, UserId: {UserId}",
                notification.ComparisonId, notification.UserId);
            throw;
        }
    }
}
