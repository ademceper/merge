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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Inventory.Inventory>;

namespace Merge.Application.Catalog.Commands.TransferStock;

public class TransferStockCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    ILogger<TransferStockCommandHandler> logger) : IRequestHandler<TransferStockCommand, bool>
{
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<bool> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Transferring stock. ProductId: {ProductId}, From: {FromWarehouse}, To: {ToWarehouse}, Quantity: {Quantity}, UserId: {UserId}",
            request.ProductId, request.FromWarehouseId, request.ToWarehouseId, request.Quantity, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Transfer miktarı 0'dan büyük olmalıdır.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to transfer stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok transfer yetkiniz yok.");
            }

            // Get source inventory
            var sourceInventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.FromWarehouseId, cancellationToken);

            if (sourceInventory == null)
            {
                logger.LogWarning("Source inventory not found. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.FromWarehouseId);
                throw new NotFoundException("Kaynak envanter", Guid.Empty);
            }

            if (sourceInventory.AvailableQuantity < request.Quantity)
            {
                logger.LogWarning("Insufficient stock for transfer. Available: {Available}, Requested: {Requested}",
                    sourceInventory.AvailableQuantity, request.Quantity);
                throw new BusinessException("Transfer için yeterli stok yok.");
            }

            // Get or create destination inventory
            var destInventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.ToWarehouseId, cancellationToken);

            if (destInventory == null)
            {
                destInventory = Inventory.Create(
                    request.ProductId,
                    request.ToWarehouseId,
                    0,
                    sourceInventory.MinimumStockLevel,
                    sourceInventory.MaximumStockLevel,
                    sourceInventory.UnitCost,
                    null);
                destInventory = await inventoryRepository.AddAsync(destInventory, cancellationToken);
            }

            var sourceQuantityBefore = sourceInventory.Quantity;
            sourceInventory.AdjustQuantity(-request.Quantity);

            var destQuantityBefore = destInventory.Quantity;
            destInventory.AdjustQuantity(request.Quantity);

            await inventoryRepository.UpdateAsync(sourceInventory, cancellationToken);
            await inventoryRepository.UpdateAsync(destInventory, cancellationToken);

            var sourceMovement = StockMovement.Create(
                sourceInventory.Id,
                request.ProductId,
                request.FromWarehouseId,
                StockMovementType.Transfer,
                request.Quantity, // Absolute value, negative is handled by MovementType
                sourceQuantityBefore,
                sourceInventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                request.FromWarehouseId,
                request.ToWarehouseId);

            var destMovement = StockMovement.Create(
                destInventory.Id,
                request.ProductId,
                request.ToWarehouseId,
                StockMovementType.Transfer,
                request.Quantity,
                destQuantityBefore,
                destInventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                request.FromWarehouseId,
                request.ToWarehouseId);

            await context.Set<StockMovement>().AddAsync(sourceMovement, cancellationToken);
            await context.Set<StockMovement>().AddAsync(destMovement, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully transferred stock. ProductId: {ProductId}, Quantity: {Quantity}, From: {FromWarehouse}, To: {ToWarehouse}",
                request.ProductId, request.Quantity, request.FromWarehouseId, request.ToWarehouseId);

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.FromWarehouseId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.ToWarehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict during stock transfer for ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok transfer çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error transferring stock for ProductId: {ProductId}", request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

