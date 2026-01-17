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
public class ProcessPayoutCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ProcessPayoutCommandHandler> logger) : IRequestHandler<ProcessPayoutCommand, bool>
{

    public async Task<bool> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Processing payout. PayoutId: {PayoutId}, TransactionReference: {TransactionReference}",
            request.PayoutId, request.TransactionReference);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout == null)
        {
            logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payout.Process(request.TransactionReference);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payout processed. PayoutId: {PayoutId}", request.PayoutId);

        return true;
    }
}
