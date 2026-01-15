using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Comparison Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductComparisonCreatedEventHandler(
    ILogger<ProductComparisonCreatedEventHandler> logger) : INotificationHandler<ProductComparisonCreatedEvent>
{

    public async Task Handle(ProductComparisonCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product comparison created event received. ComparisonId: {ComparisonId}, UserId: {UserId}, Name: {Name}, ProductCount: {ProductCount}",
            notification.ComparisonId, notification.UserId, notification.Name, notification.ProductCount);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Analytics tracking (comparison creation metrics)
            // - Cache invalidation (user comparisons cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductComparisonCreatedEvent. ComparisonId: {ComparisonId}, UserId: {UserId}",
                notification.ComparisonId, notification.UserId);
            throw;
        }
    }
}
