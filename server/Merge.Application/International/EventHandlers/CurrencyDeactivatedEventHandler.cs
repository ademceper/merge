using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyDeactivatedEventHandler : INotificationHandler<CurrencyDeactivatedEvent>
{
    private readonly ILogger<CurrencyDeactivatedEventHandler> _logger;

    public CurrencyDeactivatedEventHandler(ILogger<CurrencyDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency deactivated event received. CurrencyId: {CurrencyId}, Code: {Code}",
            notification.CurrencyId, notification.Code);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (active currencies cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyDeactivatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
