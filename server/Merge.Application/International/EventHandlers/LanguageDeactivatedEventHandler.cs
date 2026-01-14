using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Language Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class LanguageDeactivatedEventHandler : INotificationHandler<LanguageDeactivatedEvent>
{
    private readonly ILogger<LanguageDeactivatedEventHandler> _logger;

    public LanguageDeactivatedEventHandler(ILogger<LanguageDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LanguageDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Language deactivated event received. LanguageId: {LanguageId}, Code: {Code}",
            notification.LanguageId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active languages cache)
            // - Analytics tracking (language deactivation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling LanguageDeactivatedEvent. LanguageId: {LanguageId}, Code: {Code}",
                notification.LanguageId, notification.Code);
            throw;
        }
    }
}
