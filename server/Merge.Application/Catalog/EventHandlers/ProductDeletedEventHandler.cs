using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductDeletedEventHandler(
    ILogger<ProductDeletedEventHandler> logger) : INotificationHandler<ProductDeletedEvent>
{

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product deleted event received. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
            notification.ProductId, notification.Name, notification.SKU);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (products cache, category products cache, search index cache)
            // - Analytics tracking (product deletion metrics)
            // - Search index update (Elasticsearch, Algolia - remove product)
            // - External system integration (PIM, ERP, Inventory system)
            // - Notification gönderimi (seller'a ürün silindi bildirimi)
            // - Cascade delete handling (reviews, questions, variants, etc.)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductDeletedEvent. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
                notification.ProductId, notification.Name, notification.SKU);
            throw;
        }
    }
}
