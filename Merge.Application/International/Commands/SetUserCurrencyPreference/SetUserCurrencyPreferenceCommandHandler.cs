using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class SetUserCurrencyPreferenceCommandHandler : IRequestHandler<SetUserCurrencyPreferenceCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetUserCurrencyPreferenceCommandHandler> _logger;

    public SetUserCurrencyPreferenceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetUserCurrencyPreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SetUserCurrencyPreferenceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting user currency preference. UserId: {UserId}, CurrencyCode: {CurrencyCode}", 
            request.UserId, request.CurrencyCode);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CurrencyCode.ToUpper() && c.IsActive, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var preference = await _context.Set<UserCurrencyPreference>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            preference = UserCurrencyPreference.Create(
                request.UserId,
                currency.Id,
                currency.Code);
            await _context.Set<UserCurrencyPreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            preference.UpdateCurrency(currency.Id, currency.Code);
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User currency preference set successfully. UserId: {UserId}, CurrencyCode: {CurrencyCode}", 
            request.UserId, request.CurrencyCode);
        return Unit.Value;
    }
}

