using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// Currency Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CurrencyUpdatedEventHandler : INotificationHandler<CurrencyUpdatedEvent>
{
    private readonly ILogger<CurrencyUpdatedEventHandler> _logger;

    public CurrencyUpdatedEventHandler(ILogger<CurrencyUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CurrencyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Currency updated event received. CurrencyId: {CurrencyId}, Code: {Code}, Name: {Name}",
            notification.CurrencyId, notification.Code, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (currencies cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling CurrencyUpdatedEvent. CurrencyId: {CurrencyId}, Code: {Code}",
                notification.CurrencyId, notification.Code);
            throw;
        }
    }
}
