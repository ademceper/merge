using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.DeleteCommissionTier;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteCommissionTierCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteCommissionTierCommandHandler> logger) : IRequestHandler<DeleteCommissionTierCommand, bool>
{

    public async Task<bool> Handle(DeleteCommissionTierCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Deleting commission tier. TierId: {TierId}", request.TierId);

        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken);

        if (tier == null)
        {
            logger.LogWarning("Commission tier not found. TierId: {TierId}", request.TierId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        tier.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Commission tier deleted. TierId: {TierId}", request.TierId);

        return true;
    }
}
