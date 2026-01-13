using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Base Currency Status Removed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyBaseCurrencyStatusRemovedEventHandler : INotificationHandler<CurrencyBaseCurrencyStatusRemovedEvent>
{
    private readonly ILogger<CurrencyBaseCurrencyStatusRemovedEventHandler> _logger;

    public CurrencyBaseCurrencyStatusRemovedEventHandler(ILogger<CurrencyBaseCurrencyStatusRemovedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyBaseCurrencyStatusRemovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency base currency status removed event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (base currency cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyBaseCurrencyStatusRemovedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
