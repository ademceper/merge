using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// StaticTranslation Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class StaticTranslationCreatedEventHandler : INotificationHandler<StaticTranslationCreatedEvent>
{
    private readonly ILogger<StaticTranslationCreatedEventHandler> _logger;

    public StaticTranslationCreatedEventHandler(ILogger<StaticTranslationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(StaticTranslationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Static translation created event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}, Category: {Category}",
            notification.TranslationId, notification.Key, notification.LanguageCode, notification.Category);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache, UI translations cache)
            // - Analytics tracking (translation creation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling StaticTranslationCreatedEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.TranslationId, notification.Key);
            throw;
        }
    }
}
