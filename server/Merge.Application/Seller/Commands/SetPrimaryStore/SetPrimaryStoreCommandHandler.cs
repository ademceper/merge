using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SetPrimaryStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SetPrimaryStoreCommandHandler> logger) : IRequestHandler<SetPrimaryStoreCommand, bool>
{

    public async Task<bool> Handle(SetPrimaryStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Setting primary store. SellerId: {SellerId}, StoreId: {StoreId}",
            request.SellerId, request.StoreId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId && s.SellerId == request.SellerId, cancellationToken);

        if (store == null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}, SellerId: {SellerId}",
                request.StoreId, request.SellerId);
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        // Unset other primary stores
        var existingPrimary = await context.Set<Store>()
            .Where(s => s.SellerId == request.SellerId && s.IsPrimary && s.Id != request.StoreId)
            .ToListAsync(cancellationToken);

        foreach (var s in existingPrimary)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            s.RemovePrimaryStatus();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.SetAsPrimary();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Primary store set. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
