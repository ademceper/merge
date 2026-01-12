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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetInventoryByProductAndWarehouseQueryHandler : IRequestHandler<GetInventoryByProductAndWarehouseQuery, InventoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInventoryByProductAndWarehouseQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Inventory changes frequently

    public GetInventoryByProductAndWarehouseQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetInventoryByProductAndWarehouseQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<InventoryDto?> Handle(GetInventoryByProductAndWarehouseQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        var cacheKey = $"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}";

        // ✅ BOLUM 10.2: Redis distributed cache for inventory by product and warehouse
        var cachedInventory = await _cache.GetAsync<InventoryDto>(cacheKey, cancellationToken);
        if (cachedInventory != null)
        {
            _logger.LogInformation("Cache hit for inventory. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            return cachedInventory;
        }

        _logger.LogInformation("Cache miss for inventory. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var inventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId, cancellationToken);

        if (inventory == null)
        {
            _logger.LogWarning("Inventory not found for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            return null;
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        if (request.PerformedBy.HasValue && inventory.Product != null && inventory.Product.SellerId != request.PerformedBy.Value)
        {
            _logger.LogWarning("Unauthorized attempt to access inventory for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                request.ProductId, request.PerformedBy.Value, inventory.Product.SellerId);
            throw new BusinessException("Bu envantere erişim yetkiniz bulunmamaktadır.");
        }

        _logger.LogInformation("Successfully retrieved inventory {InventoryId} for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            inventory.Id, request.ProductId, request.WarehouseId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var inventoryDto = _mapper.Map<InventoryDto>(inventory);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, inventoryDto, CACHE_EXPIRATION, cancellationToken);

        return inventoryDto;
    }
}

