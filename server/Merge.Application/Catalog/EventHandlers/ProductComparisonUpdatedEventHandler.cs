using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductComparisonUpdatedEventHandler(
    ILogger<ProductComparisonUpdatedEventHandler> logger) : INotificationHandler<ProductComparisonUpdatedEvent>
{

    public async Task Handle(ProductComparisonUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product comparison updated event received. ComparisonId: {ComparisonId}, UserId: {UserId}, ProductCount: {ProductCount}",
            notification.ComparisonId, notification.UserId, notification.ProductCount);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product comparisons cache)
            // - Analytics tracking (comparison update metrics)
            // - Real-time notification (SignalR, WebSocket)
            // - External system integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductComparisonUpdatedEvent. ComparisonId: {ComparisonId}, UserId: {UserId}",
                notification.ComparisonId, notification.UserId);
            throw;
        }
    }
}
