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

namespace Merge.Application.Seller.Commands.CancelCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CancelCommissionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CancelCommissionCommandHandler> logger) : IRequestHandler<CancelCommissionCommand, bool>
{

    public async Task<bool> Handle(CancelCommissionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Cancelling commission. CommissionId: {CommissionId}", request.CommissionId);

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == request.CommissionId, cancellationToken);

        if (commission == null)
        {
            logger.LogWarning("Commission not found. CommissionId: {CommissionId}", request.CommissionId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        commission.Cancel();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission cancelled. CommissionId: {CommissionId}", request.CommissionId);

        return true;
    }
}
