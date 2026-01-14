using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Restored Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageRestoredEventHandler : INotificationHandler<LanguageRestoredEvent>
{
    private readonly ILogger<LanguageRestoredEventHandler> _logger;

    public LanguageRestoredEventHandler(ILogger<LanguageRestoredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageRestoredEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Language restored event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (languages cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageRestoredEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
