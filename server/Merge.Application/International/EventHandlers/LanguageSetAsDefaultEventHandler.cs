using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class LanguageSetAsDefaultEventHandler(ILogger<LanguageSetAsDefaultEventHandler> logger) : INotificationHandler<LanguageSetAsDefaultEvent>
{
    public async Task Handle(LanguageSetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Language set as default event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (default language cache, all translations cache)
            // - Analytics tracking (default language change metrics)
            // - Notification gönderimi (admin'lere default language değişikliği bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling LanguageSetAsDefaultEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
