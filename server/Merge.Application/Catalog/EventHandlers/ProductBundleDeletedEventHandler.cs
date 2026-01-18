using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductBundleDeletedEventHandler(
    ILogger<ProductBundleDeletedEventHandler> logger) : INotificationHandler<ProductBundleDeletedEvent>
{

    public async Task Handle(ProductBundleDeletedEvent notification, CancellationToken cancellationToken)
    {
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
            logger.LogError(ex,
                "Error handling ProductBundleDeletedEvent. BundleId: {BundleId}, Name: {Name}",
                notification.BundleId, notification.Name);
            throw;
        }
    }
}
