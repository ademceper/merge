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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Queries.GetStockReportByProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetStockReportByProductQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStockReportByProductQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetStockReportByProductQuery, StockReportDto?>
{
    private const string CACHE_KEY_STOCK_REPORT_BY_PRODUCT = "stock_report_by_product_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Shorter TTL for reports

    public async Task<StockReportDto?> Handle(GetStockReportByProductQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving stock report for ProductId: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_STOCK_REPORT_BY_PRODUCT}{request.ProductId}";

        // ✅ BOLUM 10.2: Redis distributed cache for stock reports
        var cachedReport = await cache.GetAsync<StockReportDto>(cacheKey, cancellationToken);
        if (cachedReport != null)
        {
            logger.LogInformation("Cache hit for stock report. ProductId: {ProductId}", request.ProductId);
            return cachedReport;
        }

        logger.LogInformation("Cache miss for stock report. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found with Id: {ProductId}", request.ProductId);
            return null;
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stock report'una erişebilmeli
        if (request.PerformedBy.HasValue && product.SellerId != request.PerformedBy.Value)
        {
            logger.LogWarning("Unauthorized attempt to access stock report for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                request.ProductId, request.PerformedBy.Value, product.SellerId);
            throw new BusinessException("Bu ürünün stok raporuna erişim yetkiniz bulunmamaktadır.");
        }

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var inventoryCount = await context.Set<Inventory>()
            .AsNoTracking()
            .CountAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventoryCount == 0)
        {
            logger.LogWarning("No inventory found for ProductId: {ProductId}", request.ProductId);
            return null;
        }

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalQuantity = await context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId)
            .SumAsync(i => i.Quantity, cancellationToken);

        var totalReserved = await context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId)
            .SumAsync(i => i.ReservedQuantity, cancellationToken);

        var totalAvailable = await context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId)
            .SumAsync(i => i.AvailableQuantity, cancellationToken);

        var totalValue = await context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId)
            .SumAsync(i => i.Quantity * i.UnitCost, cancellationToken);

        // ✅ PERFORMANCE: Warehouse breakdown için inventory'leri yükle (AutoMapper için gerekli)
        var inventories = await context.Set<Inventory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == request.ProductId)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Successfully retrieved stock report for ProductId: {ProductId}. TotalQuantity: {TotalQuantity}",
            request.ProductId, totalQuantity);

        // ✅ BOLUM 7.1.5: Records - Positional constructor kullanımı
        var stockReportDto = new StockReportDto(
            request.ProductId,
            product.Name,
            product.SKU,
            totalQuantity,
            totalReserved,
            totalAvailable,
            totalValue,
            mapper.Map<List<InventoryDto>>(inventories)
        );

        // Cache the result
        await cache.SetAsync(cacheKey, stockReportDto, CACHE_EXPIRATION, cancellationToken);

        return stockReportDto;
    }
}

