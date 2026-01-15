using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Bundle Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductBundleDeletedEventHandler(
    ILogger<ProductBundleDeletedEventHandler> logger) : INotificationHandler<ProductBundleDeletedEvent>
{

    public async Task Handle(ProductBundleDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Product bundle deleted event received. BundleId: {BundleId}, Name: {Name}",
            notification.BundleId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product bundles cache)
            // - Analytics tracking (bundle deletion metrics)
            // - Search index update (remove bundle)
            // - External system integration
            // - Cascade delete handling (bundle items)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling ProductBundleDeletedEvent. BundleId: {BundleId}, Name: {Name}",
                notification.BundleId, notification.Name);
            throw;
        }
    }
}
