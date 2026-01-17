using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.DeleteStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteStoreCommandHandler> logger) : IRequestHandler<DeleteStoreCommand, bool>
{

    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Deleting store. StoreId: {StoreId}", request.StoreId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Check if store has products
        var hasProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == request.StoreId, cancellationToken);

        if (hasProducts)
        {
            logger.LogWarning("Store deletion failed - Store has products. StoreId: {StoreId}", request.StoreId);
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Delete();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store deleted. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
