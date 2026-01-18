using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

namespace Merge.Application.Catalog.Queries.GetAvailableStock;

public class GetAvailableStockQueryHandler(
    IDbContext context,
    ILogger<GetAvailableStockQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetAvailableStockQuery, AvailableStockDto>
{
    private const string CACHE_KEY_AVAILABLE_STOCK = "available_stock_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(1); // Very short TTL for available stock

    public async Task<AvailableStockDto> Handle(GetAvailableStockQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving available stock for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        var cacheKey = $"{CACHE_KEY_AVAILABLE_STOCK}{request.ProductId}_{request.WarehouseId ?? Guid.Empty}";

        var cachedAvailableStock = await cache.GetAsync<AvailableStockDto>(cacheKey, cancellationToken);
        if (cachedAvailableStock is not null)
        {
            logger.LogInformation("Cache hit for available stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            return cachedAvailableStock;
        }

        logger.LogInformation("Cache miss for available stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.WarehouseId);

        if (request.PerformedBy.HasValue)
        {
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                logger.LogWarning("Product not found with Id: {ProductId}", request.ProductId);
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to access available stock for product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.ProductId, request.PerformedBy.Value, product.SellerId);
                throw new BusinessException("Bu ürünün stok bilgisine erişim yetkiniz bulunmamaktadır.");
            }
        }

        var query = context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == request.ProductId);

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == request.WarehouseId.Value);
        }

        var availableStock = await query.SumAsync(i => i.AvailableQuantity, cancellationToken);

        logger.LogInformation("Available stock for ProductId: {ProductId}, WarehouseId: {WarehouseId} is {AvailableStock}",
            request.ProductId, request.WarehouseId, availableStock);

        var availableStockDto = new AvailableStockDto(
            request.ProductId,
            request.WarehouseId,
            availableStock
        );

        // Cache the result
        await cache.SetAsync(cacheKey, availableStockDto, CACHE_EXPIRATION, cancellationToken);

        return availableStockDto;
    }
}

