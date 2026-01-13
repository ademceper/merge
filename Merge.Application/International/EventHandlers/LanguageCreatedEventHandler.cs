using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageCreatedEventHandler : INotificationHandler<LanguageCreatedEvent>
{
    private readonly ILogger<LanguageCreatedEventHandler> _logger;

    public LanguageCreatedEventHandler(ILogger<LanguageCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Language created event received. LanguageId: {LanguageId}, Code: {Code}, Name: {Name}",
            notification.LanguageId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache, translations cache)
            // - Analytics tracking (language creation metrics)
            // - Search index update (Elasticsearch, Algolia)
            // - External system integration (CMS, Translation services)
            // - Notification gönderimi (admin'lere yeni dil oluşturuldu bildirimi)
            // - Default language değişikliği durumunda tüm cache'leri temizle

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageCreatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
