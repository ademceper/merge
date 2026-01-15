using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Bundle Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductBundleCreatedEventHandler(
    ILogger<ProductBundleCreatedEventHandler> logger) : INotificationHandler<ProductBundleCreatedEvent>
{

    public async Task Handle(ProductBundleCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product bundle created event received. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}, DiscountPercentage: {DiscountPercentage}",
            notification.BundleId, notification.Name, notification.BundlePrice, notification.DiscountPercentage);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product bundles cache)
            // - Analytics tracking (bundle creation metrics)
            // - Search index update
            // - External system integration

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductBundleCreatedEvent. BundleId: {BundleId}, Name: {Name}",
                notification.BundleId, notification.Name);
            throw;
        }
    }
}
