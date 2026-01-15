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

namespace Merge.Application.Catalog.Queries.GetLowStockAlerts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetLowStockAlertsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLowStockAlertsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetLowStockAlertsQuery, PagedResult<LowStockAlertDto>>
{
    private const string CACHE_KEY_LOW_STOCK_ALERTS = "low_stock_alerts_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Short TTL for alerts

    public async Task<PagedResult<LowStockAlertDto>> Handle(GetLowStockAlertsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving low stock alerts by UserId: {UserId}. WarehouseId: {WarehouseId}, Page: {Page}, PageSize: {PageSize}",
            request.PerformedBy, request.WarehouseId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_LOW_STOCK_ALERTS}{request.PerformedBy}_{request.WarehouseId ?? Guid.Empty}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for low stock alerts
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for low stock alerts. UserId: {UserId}, WarehouseId: {WarehouseId}, Page: {Page}, PageSize: {PageSize}",
                    request.PerformedBy, request.WarehouseId, page, pageSize);

                // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin düşük stok uyarılarını görebilmeli
                var query = context.Set<Inventory>()
                    .AsNoTracking()
            .AsSplitQuery()
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.Quantity <= i.MinimumStockLevel);

                // PerformedBy null değilse (Seller), sadece kendi ürünlerini filtrele
                // PerformedBy null ise (Admin), tüm uyarıları göster
                if (request.PerformedBy != Guid.Empty)
                {
                    // Sadece kullanıcının sahip olduğu ürünlerin düşük stok uyarılarını filtrele
                    query = query.Where(i => i.Product != null && i.Product.SellerId == request.PerformedBy);
                }

                if (request.WarehouseId.HasValue)
                {
                    query = query.Where(i => i.WarehouseId == request.WarehouseId.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var lowStockItems = await query
                    .OrderBy(i => i.Quantity)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} low stock alerts (page {Page})", lowStockItems.Count, page);

                // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
                return new PagedResult<LowStockAlertDto>
                {
                    Items = mapper.Map<List<LowStockAlertDto>>(lowStockItems),
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

