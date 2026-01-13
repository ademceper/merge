using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// CategoryTranslation Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryTranslationUpdatedEventHandler : INotificationHandler<CategoryTranslationUpdatedEvent>
{
    private readonly ILogger<CategoryTranslationUpdatedEventHandler> _logger;

    public CategoryTranslationUpdatedEventHandler(ILogger<CategoryTranslationUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CategoryTranslationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Category translation updated event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageCode: {LanguageCode}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Search index update (Elasticsearch, Algolia)
            // - SEO metadata update

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CategoryTranslationUpdatedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
