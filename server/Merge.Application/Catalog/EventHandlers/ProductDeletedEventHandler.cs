using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    public ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductDeletedEvent. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
                notification.ProductId, notification.Name, notification.SKU);
            throw;
        }
    }
}
