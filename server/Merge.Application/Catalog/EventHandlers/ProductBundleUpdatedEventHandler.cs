using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Bundle Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductBundleUpdatedEventHandler(
    ILogger<ProductBundleUpdatedEventHandler> logger) : INotificationHandler<ProductBundleUpdatedEvent>
{

    public async Task Handle(ProductBundleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product bundle updated event received. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}",
            notification.BundleId, notification.Name, notification.BundlePrice);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product bundles cache)
            // - Analytics tracking (bundle update metrics)
            // - Search index update
            // - External system integration
            // - Price change alerts (if bundle price changed)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductBundleUpdatedEvent. BundleId: {BundleId}, Name: {Name}",
                notification.BundleId, notification.Name);
            throw;
        }
    }
}
