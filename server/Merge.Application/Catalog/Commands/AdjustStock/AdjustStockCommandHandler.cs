using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
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

namespace Merge.Application.Catalog.Commands.AdjustStock;

public class AdjustStockCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    IMapper mapper,
    ILogger<AdjustStockCommandHandler> logger) : IRequestHandler<AdjustStockCommand, InventoryDto>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<InventoryDto> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adjusting stock for InventoryId: {InventoryId}, QuantityChange: {QuantityChange}",
            request.InventoryId, request.QuantityChange);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var inventory = await context.Set<Inventory>()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);

            if (inventory == null)
            {
                logger.LogWarning("Inventory not found for adjustment with Id: {InventoryId}", request.InventoryId);
                throw new NotFoundException("Envanter", request.InventoryId);
            }

            if (inventory.Product.SellerId.HasValue && inventory.Product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to adjust stock for inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.InventoryId, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            var quantityBefore = inventory.Quantity;
            inventory.AdjustQuantity(request.QuantityChange);
            var quantityAfter = inventory.Quantity;

            await inventoryRepository.UpdateAsync(inventory, cancellationToken);

            var stockMovement = StockMovement.Create(
                inventory.Id,
                inventory.ProductId,
                inventory.WarehouseId,
                StockMovementType.Adjustment,
                Math.Abs(request.QuantityChange),
                quantityBefore,
                quantityAfter,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                null, // fromWarehouseId
                null); // toWarehouseId

            await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully adjusted stock for InventoryId: {InventoryId}. Before: {Before}, After: {After}",
                request.InventoryId, quantityBefore, quantityAfter);

            // Reload with includes for AutoMapper
            inventory = await context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.InventoryId}", cancellationToken);
            if (inventory != null)
            {
                await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{inventory.ProductId}_{inventory.WarehouseId}", cancellationToken);
                await cache.RemoveAsync($"inventories_by_product_{inventory.ProductId}", cancellationToken); // Invalidate product inventories list cache
            }

            return mapper.Map<InventoryDto>(inventory!);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while adjusting stock for InventoryId: {InventoryId}", request.InventoryId);
            throw new BusinessException("Stok güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adjusting stock for InventoryId: {InventoryId}", request.InventoryId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

