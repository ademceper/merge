using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageDeletedEventHandler : INotificationHandler<LanguageDeletedEvent>
{
    private readonly ILogger<LanguageDeletedEventHandler> _logger;

    public LanguageDeletedEventHandler(ILogger<LanguageDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Language deleted event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache, translations cache)
            // - Analytics tracking (language deletion metrics)
            // - Search index update (Elasticsearch, Algolia)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageDeletedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
