using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Set As Default Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageSetAsDefaultEventHandler : INotificationHandler<LanguageSetAsDefaultEvent>
{
    private readonly ILogger<LanguageSetAsDefaultEventHandler> _logger;

    public LanguageSetAsDefaultEventHandler(ILogger<LanguageSetAsDefaultEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageSetAsDefaultEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageSetAsDefaultEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
