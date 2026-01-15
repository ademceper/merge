using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class CurrencyExchangeRateUpdatedEventHandler(ILogger<CurrencyExchangeRateUpdatedEventHandler> logger) : INotificationHandler<CurrencyExchangeRateUpdatedEvent>
{
    public async Task Handle(CurrencyExchangeRateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling CurrencyExchangeRateUpdatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
