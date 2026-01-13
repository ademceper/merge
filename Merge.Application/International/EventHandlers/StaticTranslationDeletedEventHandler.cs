using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// StaticTranslation Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class StaticTranslationDeletedEventHandler : INotificationHandler<StaticTranslationDeletedEvent>
{
    private readonly ILogger<StaticTranslationDeletedEventHandler> _logger;

    public StaticTranslationDeletedEventHandler(ILogger<StaticTranslationDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(StaticTranslationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Static translation deleted event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}",
            notification.TranslationId, notification.Key, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache, UI translations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling StaticTranslationDeletedEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.TranslationId, notification.Key);
            throw;
        }
    }
}
