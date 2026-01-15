using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Queries.GetInventoriesByWarehouseId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoriesByWarehouseIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetInventoriesByWarehouseIdQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetInventoriesByWarehouseIdQuery, PagedResult<InventoryDto>>
{
    private const string CACHE_KEY_INVENTORIES_BY_WAREHOUSE = "inventories_by_warehouse_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Inventory changes frequently

    public async Task<PagedResult<InventoryDto>> Handle(GetInventoriesByWarehouseIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving inventories for WarehouseId: {WarehouseId} by UserId: {UserId}. Page: {Page}, PageSize: {PageSize}",
            request.WarehouseId, request.PerformedBy, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_INVENTORIES_BY_WAREHOUSE}{request.WarehouseId}_{request.PerformedBy}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated inventory queries
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for inventories by warehouse. WarehouseId: {WarehouseId}, UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                    request.WarehouseId, request.PerformedBy, page, pageSize);

                // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
                var query = context.Set<Inventory>()
                    .AsNoTracking()
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.WarehouseId == request.WarehouseId);

                // PerformedBy null değilse (Seller), sadece kendi ürünlerini filtrele
                // PerformedBy null ise (Admin), tüm envanterleri göster
                if (request.PerformedBy != Guid.Empty)
                {
                    // Sadece kullanıcının sahip olduğu ürünlerin envanterlerini filtrele
                    query = query.Where(i => i.Product != null && i.Product.SellerId == request.PerformedBy);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var inventories = await query
                    .OrderBy(i => i.Product.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} inventories for WarehouseId: {WarehouseId} (page {Page})",
                    inventories.Count, request.WarehouseId, page);

                return new PagedResult<InventoryDto>
                {
                    Items = mapper.Map<List<InventoryDto>>(inventories),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
    }
}

