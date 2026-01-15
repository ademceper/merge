using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Commands.ReleaseStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ReleaseStockCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    Merge.Application.Interfaces.IRepository<Inventory> inventoryRepository,
    ICacheService cache,
    ILogger<ReleaseStockCommandHandler> logger) : IRequestHandler<ReleaseStockCommand, bool>
{
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<bool> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Releasing stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}, UserId: {UserId}",
            request.ProductId, request.WarehouseId, request.Quantity, request.OrderId, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Serbest bırakılacak miktar 0'dan büyük olmalıdır.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory + StockMovement)
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stokunu serbest bırakabilmeli
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to release stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok serbest bırakma yetkiniz yok.");
            }

            // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
            var inventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (validation entity içinde)
            var quantityBefore = inventory.Quantity;
            inventory.ReleaseAndReduce(request.Quantity);

            await inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var stockMovement = StockMovement.Create(
                inventory.Id,
                request.ProductId,
                request.WarehouseId,
                StockMovementType.Sale,
                request.Quantity, // Absolute value, negative is handled by MovementType
                quantityBefore,
                inventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                request.OrderId, // referenceId
                $"Stock released for order fulfillment {request.OrderId}",
                null, // fromWarehouseId
                null); // toWarehouseId

            await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully released stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                request.ProductId, request.Quantity, request.OrderId);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while releasing stock. ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok serbest bırakma çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error releasing stock. ProductId: {ProductId}", request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

