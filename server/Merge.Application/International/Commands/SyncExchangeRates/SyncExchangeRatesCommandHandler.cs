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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class SyncExchangeRatesCommandHandler : IRequestHandler<SyncExchangeRatesCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncExchangeRatesCommandHandler> _logger;

    public SyncExchangeRatesCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SyncExchangeRatesCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SyncExchangeRatesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Syncing exchange rates");

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currencies = await _context.Set<Currency>()
            .Where(c => c.IsActive && !c.IsBaseCurrency)
            .ToListAsync(cancellationToken);

        // TODO: İleride burada harici API entegrasyonu yapılacak (e.g., exchangerate-api.com, fixer.io)
        // Şimdilik sadece loglama yapıyoruz
        foreach (var currency in currencies)
        {
            _logger.LogInformation("Processing currency. Code: {Code}, CurrentRate: {Rate}", 
                currency.Code, currency.ExchangeRate);
            
            // Placeholder: Harici API'den güncel kur bilgisi alınacak
            // var newRate = await _exchangeRateApi.GetRateAsync(currency.Code, cancellationToken);
            // currency.UpdateExchangeRate(newRate, "API");
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exchange rates sync completed. Processed: {Count}", currencies.Count);
        return Unit.Value;
    }
}

