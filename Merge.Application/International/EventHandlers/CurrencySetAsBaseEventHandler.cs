using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Set As Base Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencySetAsBaseEventHandler : INotificationHandler<CurrencySetAsBaseEvent>
{
    private readonly ILogger<CurrencySetAsBaseEventHandler> _logger;

    public CurrencySetAsBaseEventHandler(ILogger<CurrencySetAsBaseEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencySetAsBaseEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency set as base event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (base currency cache, all exchange rates cache)
            // - Analytics tracking (base currency change metrics)
            // - Notification gönderimi (admin'lere base currency değişikliği bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencySetAsBaseEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
