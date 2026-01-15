using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Commands.CreateInventory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateInventoryCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateInventoryCommandHandler> logger) : IRequestHandler<CreateInventoryCommand, InventoryDto>
{
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<InventoryDto> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}",
            request.ProductId, request.WarehouseId, request.Quantity);

        if (request.Quantity < 0)
        {
            throw new ValidationException("Miktar negatif olamaz.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory + StockMovement)
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini oluşturabilmeli
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // Seller kontrolü: Seller sadece kendi ürünlerinin inventory'sini oluşturabilir
            // Admin tüm ürünler için inventory oluşturabilir
            // Note: Role bilgisi command'da yok, bu yüzden sadece SellerId kontrolü yapıyoruz
            // Controller seviyesinde role kontrolü yapılıyor, burada sadece ownership kontrolü
            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to create inventory for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için envanter oluşturma yetkiniz yok.");
            }

            // Check if inventory already exists for this product-warehouse combination
            var existingInventory = await context.Set<Inventory>()
                .AsNoTracking()
                .AnyAsync(i => i.ProductId == request.ProductId &&
                              i.WarehouseId == request.WarehouseId, cancellationToken);

            if (existingInventory)
            {
                logger.LogWarning("Attempted to create duplicate inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.WarehouseId);
                throw new BusinessException("Bu ürün-depo kombinasyonu için envanter zaten mevcut.");
            }

            // ✅ BOLUM 1.1: Factory Method kullanımı
            var inventory = Inventory.Create(
                request.ProductId,
                request.WarehouseId,
                request.Quantity,
                request.MinimumStockLevel,
                request.MaximumStockLevel,
                request.UnitCost,
                request.Location);

            await context.Set<Inventory>().AddAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // Create initial stock movement
            if (request.Quantity > 0)
            {
                var stockMovement = StockMovement.Create(
                    inventory.Id,
                    inventory.ProductId,
                    inventory.WarehouseId,
                    StockMovementType.Receipt,
                    request.Quantity,
                    0, // quantityBefore
                    request.Quantity, // quantityAfter
                    request.PerformedBy,
                    null, // referenceNumber
                    null, // referenceId
                    "Initial inventory creation",
                    null, // fromWarehouseId
                    null); // toWarehouseId

                await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);
            }

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            var reloadedInventory = await context.Set<Inventory>()
                .AsNoTracking()
            .AsSplitQuery()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == inventory.Id, cancellationToken);

            if (reloadedInventory == null)
            {
                logger.LogWarning("Inventory {InventoryId} not found after creation", inventory.Id);
                return mapper.Map<InventoryDto>(inventory);
            }

            logger.LogInformation("Successfully created inventory with Id: {InventoryId} for ProductId: {ProductId}",
                reloadedInventory.Id, request.ProductId);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{reloadedInventory.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return mapper.Map<InventoryDto>(reloadedInventory);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error creating inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

