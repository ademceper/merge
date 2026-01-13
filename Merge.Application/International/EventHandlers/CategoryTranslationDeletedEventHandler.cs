using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// CategoryTranslation Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CategoryTranslationDeletedEventHandler : INotificationHandler<CategoryTranslationDeletedEvent>
{
    private readonly ILogger<CategoryTranslationDeletedEventHandler> _logger;

    public CategoryTranslationDeletedEventHandler(ILogger<CategoryTranslationDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CategoryTranslationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Category translation deleted event received. TranslationId: {TranslationId}, CategoryId: {CategoryId}, LanguageCode: {LanguageCode}",
            notification.CategoryTranslationId, notification.CategoryId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (category translations cache, category cache)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CategoryTranslationDeletedEvent. TranslationId: {TranslationId}, CategoryId: {CategoryId}",
                notification.CategoryTranslationId, notification.CategoryId);
            throw;
        }
    }
}
