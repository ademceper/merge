using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateExchangeRateCommandHandler> _logger;

    public UpdateExchangeRateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateExchangeRateCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating exchange rate. CurrencyCode: {CurrencyCode}, NewRate: {NewRate}, Source: {Source}", 
            request.CurrencyCode, request.NewRate, request.Source);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CurrencyCode.ToUpper(), cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        currency.UpdateExchangeRate(request.NewRate, request.Source);

        // Save to history
        var history = ExchangeRateHistory.Create(
            currency.Id,
            currency.Code,
            request.NewRate,
            request.Source);

        await _context.Set<ExchangeRateHistory>().AddAsync(history, cancellationToken);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exchange rate updated successfully. CurrencyCode: {CurrencyCode}, NewRate: {NewRate}", 
            request.CurrencyCode, request.NewRate);
        return Unit.Value;
    }
}

