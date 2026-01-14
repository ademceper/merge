using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// CategoryTranslation Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryTranslationCreatedEventHandler : INotificationHandler<CategoryTranslationCreatedEvent>
{
    private readonly ILogger<CategoryTranslationCreatedEventHandler> _logger;

    public CategoryTranslationCreatedEventHandler(ILogger<CategoryTranslationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CategoryTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Category translation created event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}, CategoryName: {CategoryName}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageId, notification.LanguageCode, notification.CategoryName);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Analytics tracking (translation creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CategoryTranslationCreatedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
