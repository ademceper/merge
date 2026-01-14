using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.ProcessPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ProcessPayoutCommandHandler : IRequestHandler<ProcessPayoutCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPayoutCommandHandler> _logger;

    public ProcessPayoutCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPayoutCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Processing payout. PayoutId: {PayoutId}, TransactionReference: {TransactionReference}",
            request.PayoutId, request.TransactionReference);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout == null)
        {
            _logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payout.Process(request.TransactionReference);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payout processed. PayoutId: {PayoutId}", request.PayoutId);

        return true;
    }
}
