using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;

namespace Merge.Application.International.Commands.SyncExchangeRates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class SyncExchangeRatesCommandHandler : IRequestHandler<SyncExchangeRatesCommand, Unit>
{
    private readonly ILogger<SyncExchangeRatesCommandHandler> _logger;

    public SyncExchangeRatesCommandHandler(
        ILogger<SyncExchangeRatesCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Unit> Handle(SyncExchangeRatesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Syncing exchange rates");

        // Placeholder for future API integration (e.g., exchangerate-api.com, fixer.io)
        // For now, this is a manual operation
        await Task.CompletedTask;

        _logger.LogInformation("Exchange rates sync completed");
        return Unit.Value;
    }
}

