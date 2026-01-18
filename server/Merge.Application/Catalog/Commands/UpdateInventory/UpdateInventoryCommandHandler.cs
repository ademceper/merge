using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Inventory.Inventory>;

namespace Merge.Application.Catalog.Commands.UpdateInventory;

public class UpdateInventoryCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    IMapper mapper,
    ILogger<UpdateInventoryCommandHandler> logger) : IRequestHandler<UpdateInventoryCommand, InventoryDto>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<InventoryDto> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating inventory Id: {InventoryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await context.Set<Inventory>()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (inventory is null)
            {
                logger.LogWarning("Inventory not found with Id: {InventoryId}", request.Id);
                throw new NotFoundException("Envanter", request.Id);
            }

            if (inventory.Product.SellerId.HasValue && inventory.Product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to update inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.Id, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            inventory.UpdateStockLevels(request.MinimumStockLevel, request.MaximumStockLevel);
            inventory.UpdateUnitCost(request.UnitCost);
            inventory.UpdateLocation(request.Location);

            await inventoryRepository.UpdateAsync(inventory, cancellationToken);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            inventory = await context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            logger.LogInformation("Successfully updated inventory Id: {InventoryId}", request.Id);

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.Id}", cancellationToken);
            if (inventory is not null)
            {
                await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{inventory.ProductId}_{inventory.WarehouseId}", cancellationToken);
                await cache.RemoveAsync($"inventories_by_product_{inventory.ProductId}", cancellationToken); // Invalidate product inventories list cache
            }

            return mapper.Map<InventoryDto>(inventory!);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating inventory Id: {InventoryId}", request.Id);
            throw new BusinessException("Envanter güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating inventory Id: {InventoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

