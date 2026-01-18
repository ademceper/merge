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
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Queries.GetInventoryByProductAndWarehouse;

public class GetInventoryByProductAndWarehouseQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetInventoryByProductAndWarehouseQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetInventoryByProductAndWarehouseQuery, InventoryDto?>
{
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Inventory changes frequently

    public async Task<InventoryDto?> Handle(GetInventoryByProductAndWarehouseQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        var cacheKey = $"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}";

        var cachedInventory = await cache.GetAsync<InventoryDto>(cacheKey, cancellationToken);
        if (cachedInventory is not null)
        {
            logger.LogInformation("Cache hit for inventory. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            return cachedInventory;
        }

        logger.LogInformation("Cache miss for inventory. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        var inventory = await context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId, cancellationToken);

        if (inventory is null)
        {
            logger.LogWarning("Inventory not found for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            return null;
        }

        if (request.PerformedBy.HasValue && inventory.Product is not null && inventory.Product.SellerId != request.PerformedBy.Value)
        {
            logger.LogWarning("Unauthorized attempt to access inventory for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                request.ProductId, request.PerformedBy.Value, inventory.Product.SellerId);
            throw new BusinessException("Bu envantere erişim yetkiniz bulunmamaktadır.");
        }

        logger.LogInformation("Successfully retrieved inventory {InventoryId} for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            inventory.Id, request.ProductId, request.WarehouseId);

        var inventoryDto = mapper.Map<InventoryDto>(inventory);
        
        // Cache the result
        await cache.SetAsync(cacheKey, inventoryDto, CACHE_EXPIRATION, cancellationToken);

        return inventoryDto;
    }
}

