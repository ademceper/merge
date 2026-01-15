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

namespace Merge.Application.Catalog.Queries.GetInventoriesByProductId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoriesByProductIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetInventoriesByProductIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetInventoriesByProductIdQuery, IEnumerable<InventoryDto>>
{
    private const string CACHE_KEY_INVENTORIES_BY_PRODUCT = "inventories_by_product_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Shorter TTL for inventory lists

    public async Task<IEnumerable<InventoryDto>> Handle(GetInventoriesByProductIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving inventories for ProductId: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_INVENTORIES_BY_PRODUCT}{request.ProductId}";

        // ✅ BOLUM 10.2: Redis distributed cache for inventory lists
        var cachedInventories = await cache.GetAsync<IEnumerable<InventoryDto>>(cacheKey, cancellationToken);
        if (cachedInventories != null)
        {
            logger.LogInformation("Cache hit for inventories by product. ProductId: {ProductId}", request.ProductId);
            return cachedInventories;
        }

        logger.LogInformation("Cache miss for inventories by product. ProductId: {ProductId}", request.ProductId);

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        if (request.PerformedBy.HasValue)
        {
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                logger.LogWarning("Product not found with Id: {ProductId}", request.ProductId);
                return Enumerable.Empty<InventoryDto>();
            }

            if (product.SellerId != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to access inventories for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.ProductId, request.PerformedBy.Value, product.SellerId);
                throw new BusinessException("Bu ürünün envanterlerine erişim yetkiniz bulunmamaktadır.");
            }
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Maksimum limit (100 warehouse)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var inventories = await context.Set<Inventory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == request.ProductId)
            .OrderBy(i => i.Warehouse.Name)
            .Take(100) // ✅ Güvenlik: Maksimum 100 warehouse inventory'si
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} inventories for ProductId: {ProductId}",
            inventories.Count, request.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var inventoryDtos = mapper.Map<IEnumerable<InventoryDto>>(inventories);

        // Cache the result
        await cache.SetAsync(cacheKey, inventoryDtos, CACHE_EXPIRATION, cancellationToken);

        return inventoryDtos;
    }
}

