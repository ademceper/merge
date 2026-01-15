using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageCreatedEventHandler(ILogger<LanguageCreatedEventHandler> logger) : INotificationHandler<LanguageCreatedEvent>
{
    public async Task Handle(LanguageCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling LanguageCreatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
