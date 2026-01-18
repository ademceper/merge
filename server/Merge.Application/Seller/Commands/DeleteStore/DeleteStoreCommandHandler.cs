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

public class DeleteStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteStoreCommandHandler> logger) : IRequestHandler<DeleteStoreCommand, bool>
{

    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting store. StoreId: {StoreId}", request.StoreId);

        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // Check if store has products
        var hasProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == request.StoreId, cancellationToken);

        if (hasProducts)
        {
            logger.LogWarning("Store deletion failed - Store has products. StoreId: {StoreId}", request.StoreId);
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        store.Delete();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store deleted. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
