using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Comparison Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductComparisonDeletedEventHandler(
    ILogger<ProductComparisonDeletedEventHandler> logger) : INotificationHandler<ProductComparisonDeletedEvent>
{

    public async Task Handle(ProductComparisonDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductComparisonDeletedEvent. ComparisonId: {ComparisonId}, UserId: {UserId}",
                notification.ComparisonId, notification.UserId);
            throw;
        }
    }
}
