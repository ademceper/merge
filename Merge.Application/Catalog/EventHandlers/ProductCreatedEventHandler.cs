using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product created event received. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}, CategoryId: {CategoryId}, SellerId: {SellerId}",
            notification.ProductId, notification.Name, notification.SKU, notification.CategoryId, notification.SellerId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (products cache, category products cache)
            // - Analytics tracking (product creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - External system integration (PIM, ERP, Inventory system)
            // - Notification gönderimi (seller'a yeni ürün oluşturuldu bildirimi)
            // - Image processing queue'a ekleme
            // - SEO metadata generation

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductCreatedEvent. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
                notification.ProductId, notification.Name, notification.SKU);
            throw;
        }
    }
}
