using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// StaticTranslation Restored Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class StaticTranslationRestoredEventHandler : INotificationHandler<StaticTranslationRestoredEvent>
{
    private readonly ILogger<StaticTranslationRestoredEventHandler> _logger;

    public StaticTranslationRestoredEventHandler(ILogger<StaticTranslationRestoredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(StaticTranslationRestoredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Static translation restored event received. TranslationId: {TranslationId}, Key: {Key}, LanguageCode: {LanguageCode}",
            notification.Id, notification.Key, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (static translations cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling StaticTranslationRestoredEvent. TranslationId: {TranslationId}, Key: {Key}",
                notification.Id, notification.Key);
            throw;
        }
    }
}
