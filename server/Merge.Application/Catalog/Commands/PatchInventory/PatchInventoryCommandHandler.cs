using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

namespace Merge.Application.Catalog.Commands.PatchInventory;

/// <summary>
/// Handler for PatchInventoryCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchInventoryCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    IMapper mapper,
    ILogger<PatchInventoryCommandHandler> logger) : IRequestHandler<PatchInventoryCommand, InventoryDto>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<InventoryDto> Handle(PatchInventoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching inventory Id: {InventoryId}", request.Id);

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
                logger.LogWarning("IDOR attempt: User {UserId} tried to patch inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.Id, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            // Apply partial updates - get existing values if not provided
            var minimumStockLevel = request.PatchDto.MinimumStockLevel ?? inventory.MinimumStockLevel;
            var maximumStockLevel = request.PatchDto.MaximumStockLevel ?? inventory.MaximumStockLevel;
            var unitCost = request.PatchDto.UnitCost ?? inventory.UnitCost;
            var location = request.PatchDto.Location ?? inventory.Location;

            inventory.UpdateStockLevels(minimumStockLevel, maximumStockLevel);
            inventory.UpdateUnitCost(unitCost);
            inventory.UpdateLocation(location);

            await inventoryRepository.UpdateAsync(inventory, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedInventory = await context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (reloadedInventory is null)
            {
                logger.LogWarning("Inventory not found after patch. InventoryId: {InventoryId}", request.Id);
                throw new NotFoundException("Envanter", request.Id);
            }

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{inventory.ProductId}_{inventory.WarehouseId}", cancellationToken);

            logger.LogInformation("Inventory patched successfully. InventoryId: {InventoryId}", request.Id);

            return mapper.Map<InventoryDto>(reloadedInventory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching inventory. InventoryId: {InventoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
