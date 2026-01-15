using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Commands.UpdateLastCountDate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateLastCountDateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    Merge.Application.Interfaces.IRepository<Inventory> inventoryRepository,
    ICacheService cache,
    ILogger<UpdateLastCountDateCommandHandler> logger) : IRequestHandler<UpdateLastCountDateCommand, bool>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<bool> Handle(UpdateLastCountDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating last count date for InventoryId: {InventoryId} by UserId: {UserId}", request.InventoryId, request.PerformedBy);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await context.Set<Inventory>()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);
            
            if (inventory == null)
            {
                logger.LogWarning("Inventory not found with Id: {InventoryId}", request.InventoryId);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
            if (inventory.Product.SellerId.HasValue && inventory.Product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to update last count date for inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.InventoryId, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            // Store product and warehouse IDs for cache invalidation
            var productId = inventory.ProductId;
            var warehouseId = inventory.WarehouseId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            inventory.UpdateLastCountedDate();
            await inventoryRepository.UpdateAsync(inventory, cancellationToken);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully updated last count date for InventoryId: {InventoryId}", request.InventoryId);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.InventoryId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{productId}_{warehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{productId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating last count date for InventoryId: {InventoryId}", request.InventoryId);
            throw new BusinessException("Envanter sayım tarihi güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error updating last count date for InventoryId: {InventoryId}", request.InventoryId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

