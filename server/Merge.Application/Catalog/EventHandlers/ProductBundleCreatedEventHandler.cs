using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductBundleCreatedEventHandler(
    ILogger<ProductBundleCreatedEventHandler> logger) : INotificationHandler<ProductBundleCreatedEvent>
{

    public async Task Handle(ProductBundleCreatedEvent notification, CancellationToken cancellationToken)
    {
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
            logger.LogError(ex,
                "Error handling ProductBundleCreatedEvent. BundleId: {BundleId}, Name: {Name}",
                notification.BundleId, notification.Name);
            throw;
        }
    }
}
