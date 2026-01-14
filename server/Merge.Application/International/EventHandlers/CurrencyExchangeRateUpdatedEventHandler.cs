using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Exchange Rate Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyExchangeRateUpdatedEventHandler : INotificationHandler<CurrencyExchangeRateUpdatedEvent>
{
    private readonly ILogger<CurrencyExchangeRateUpdatedEventHandler> _logger;

    public CurrencyExchangeRateUpdatedEventHandler(ILogger<CurrencyExchangeRateUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyExchangeRateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency exchange rate updated event received. CurrencyId: {CurrencyId}, Code: {Code}, OldRate: {OldRate}, NewRate: {NewRate}, Source: {Source}",
            notification.CurrencyId, notification.Code, notification.OldRate, notification.NewRate, notification.Source);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (exchange rates cache, price conversion cache)
            // - Analytics tracking (exchange rate change metrics)
            // - Price recalculation for active carts (if significant change)
            // - Notification gönderimi (admin'lere önemli kur değişikliği bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyExchangeRateUpdatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
