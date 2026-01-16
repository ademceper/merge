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

namespace Merge.Application.Seller.Commands.FailPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class FailPayoutCommandHandler : IRequestHandler<FailPayoutCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FailPayoutCommandHandler> _logger;

    public FailPayoutCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<FailPayoutCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(FailPayoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Failing payout. PayoutId: {PayoutId}, Reason: {Reason}",
            request.PayoutId, request.Reason);

        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout == null)
        {
            _logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payout.Fail(request.Reason);

        // Revert commissions back to approved using domain method
        foreach (var item in payout.Items)
        {
            if (item.Commission != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                item.Commission.RevertToApproved();
            }
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payout failed. PayoutId: {PayoutId}, Reason: {Reason}",
            request.PayoutId, request.Reason);

        return true;
    }
}
