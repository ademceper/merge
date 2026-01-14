using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// ProductTranslation Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductTranslationCreatedEventHandler : INotificationHandler<ProductTranslationCreatedEvent>
{
    private readonly ILogger<ProductTranslationCreatedEventHandler> _logger;

    public ProductTranslationCreatedEventHandler(ILogger<ProductTranslationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product translation created event received. TranslationId: {TranslationId}, ProductId: {ProductId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}, ProductName: {ProductName}",
            notification.ProductTranslationId, notification.ProductId, notification.LanguageId, notification.LanguageCode, notification.ProductName);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product translations cache, product cache)
            // - Analytics tracking (translation creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductTranslationCreatedEvent. TranslationId: {TranslationId}, ProductId: {ProductId}",
                notification.ProductTranslationId, notification.ProductId);
            throw;
        }
    }
}
