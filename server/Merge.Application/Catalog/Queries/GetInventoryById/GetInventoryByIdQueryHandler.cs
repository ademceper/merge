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

namespace Merge.Application.Catalog.Queries.GetInventoryById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoryByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetInventoryByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetInventoryByIdQuery, InventoryDto?>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Inventory changes frequently

    public async Task<InventoryDto?> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving inventory with Id: {InventoryId}", request.Id);

        var cacheKey = $"{CACHE_KEY_INVENTORY_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single inventory
        var cachedInventory = await cache.GetAsync<InventoryDto>(cacheKey, cancellationToken);
        if (cachedInventory != null)
        {
            logger.LogInformation("Cache hit for inventory. InventoryId: {InventoryId}", request.Id);
            return cachedInventory;
        }

        logger.LogInformation("Cache miss for inventory. InventoryId: {InventoryId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var inventory = await context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inventory == null)
        {
            logger.LogWarning("Inventory not found with Id: {InventoryId}", request.Id);
            return null;
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        if (request.PerformedBy.HasValue && inventory.Product != null && inventory.Product.SellerId != request.PerformedBy.Value)
        {
            logger.LogWarning("Unauthorized attempt to access inventory {InventoryId} for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                request.Id, inventory.ProductId, request.PerformedBy.Value, inventory.Product.SellerId);
            throw new BusinessException("Bu envantere erişim yetkiniz bulunmamaktadır.");
        }

        logger.LogInformation("Successfully retrieved inventory {InventoryId} for ProductId: {ProductId}",
            request.Id, inventory.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var inventoryDto = mapper.Map<InventoryDto>(inventory);
        
        // Cache the result
        await cache.SetAsync(cacheKey, inventoryDto, CACHE_EXPIRATION, cancellationToken);

        return inventoryDto;
    }
}

