using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyCreatedEventHandler : INotificationHandler<CurrencyCreatedEvent>
{
    private readonly ILogger<CurrencyCreatedEventHandler> _logger;

    public CurrencyCreatedEventHandler(ILogger<CurrencyCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency created event received. CurrencyId: {CurrencyId}, Code: {Code}, Name: {Name}",
            notification.CurrencyId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache, exchange rates cache)
            // - Analytics tracking (currency creation metrics)
            // - External system integration (Payment gateways, Financial systems)
            // - Notification gönderimi (admin'lere yeni para birimi oluşturuldu bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyCreatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
