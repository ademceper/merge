using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.SyncExchangeRates;

public class SyncExchangeRatesCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<SyncExchangeRatesCommandHandler> logger) : IRequestHandler<SyncExchangeRatesCommand, Unit>
{
    public async Task<Unit> Handle(SyncExchangeRatesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Syncing exchange rates");

        var currencies = await context.Set<Currency>()
            .Where(c => c.IsActive && !c.IsBaseCurrency)
            .ToListAsync(cancellationToken);

        // TODO: İleride burada harici API entegrasyonu yapılacak (e.g., exchangerate-api.com, fixer.io)
        // Şimdilik sadece loglama yapıyoruz
        foreach (var currency in currencies)
        {
            logger.LogInformation("Processing currency. Code: {Code}, CurrentRate: {Rate}", 
                currency.Code, currency.ExchangeRate);
            
            // Placeholder: Harici API'den güncel kur bilgisi alınacak
            // var newRate = await _exchangeRateApi.GetRateAsync(currency.Code, cancellationToken);
            // currency.UpdateExchangeRate(newRate, "API");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Exchange rates sync completed. Processed: {Count}", currencies.Count);
        return Unit.Value;
    }
}
