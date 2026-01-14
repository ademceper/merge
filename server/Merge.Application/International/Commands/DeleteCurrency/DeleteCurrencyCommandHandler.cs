using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCurrencyCommandHandler> _logger;

    public DeleteCurrencyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCurrencyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting currency. CurrencyId: {CurrencyId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found. CurrencyId: {CurrencyId}", request.Id);
            throw new NotFoundException("Para birimi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        currency.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency deleted successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);
        return Unit.Value;
    }
}

