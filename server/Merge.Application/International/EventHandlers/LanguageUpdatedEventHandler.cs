using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageUpdatedEventHandler : INotificationHandler<LanguageUpdatedEvent>
{
    private readonly ILogger<LanguageUpdatedEventHandler> _logger;

    public LanguageUpdatedEventHandler(ILogger<LanguageUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Language updated event received. LanguageId: {LanguageId}, Code: {Code}, Name: {Name}",
            notification.LanguageId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache, translations cache)
            // - Analytics tracking (language update metrics)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageUpdatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
