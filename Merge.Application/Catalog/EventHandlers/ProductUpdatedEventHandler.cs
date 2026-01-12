using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedEvent>
{
    private readonly ILogger<ProductUpdatedEventHandler> _logger;

    public ProductUpdatedEventHandler(ILogger<ProductUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product updated event received. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}, CategoryId: {CategoryId}",
            notification.ProductId, notification.Name, notification.SKU, notification.CategoryId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (products cache, category products cache, search index cache)
            // - Analytics tracking (product update metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - External system integration (PIM, ERP, Inventory system)
            // - Notification gönderimi (seller'a ürün güncellendi bildirimi)
            // - Price change alerts (if price changed)
            // - Stock change alerts (if stock changed)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductUpdatedEvent. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
                notification.ProductId, notification.Name, notification.SKU);
            throw;
        }
    }
}
