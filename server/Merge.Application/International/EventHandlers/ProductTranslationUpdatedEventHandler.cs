using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// ProductTranslation Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductTranslationUpdatedEventHandler : INotificationHandler<ProductTranslationUpdatedEvent>
{
    private readonly ILogger<ProductTranslationUpdatedEventHandler> _logger;

    public ProductTranslationUpdatedEventHandler(ILogger<ProductTranslationUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductTranslationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product translation updated event received. TranslationId: {TranslationId}, ProductId: {ProductId}, LanguageCode: {LanguageCode}",
            notification.ProductTranslationId, notification.ProductId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product translations cache, product cache)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductTranslationUpdatedEvent. TranslationId: {TranslationId}, ProductId: {ProductId}",
                notification.ProductTranslationId, notification.ProductId);
            throw;
        }
    }
}
