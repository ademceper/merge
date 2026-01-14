using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyDeletedEventHandler : INotificationHandler<CurrencyDeletedEvent>
{
    private readonly ILogger<CurrencyDeletedEventHandler> _logger;

    public CurrencyDeletedEventHandler(ILogger<CurrencyDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency deleted event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache, exchange rates cache)
            // - Analytics tracking (currency deletion metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyDeletedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
