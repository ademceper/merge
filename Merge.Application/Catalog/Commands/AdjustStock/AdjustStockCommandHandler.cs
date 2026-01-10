using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Catalog.Commands.AdjustStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, InventoryDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<AdjustStockCommandHandler> _logger;
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public AdjustStockCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IRepository<Inventory> inventoryRepository,
        ICacheService cache,
        IMapper mapper,
        ILogger<AdjustStockCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InventoryDto> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adjusting stock for InventoryId: {InventoryId}, QuantityChange: {QuantityChange}",
            request.InventoryId, request.QuantityChange);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory + StockMovement)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var inventory = await _context.Set<Inventory>()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for adjustment with Id: {InventoryId}", request.InventoryId);
                throw new NotFoundException("Envanter", request.InventoryId);
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
            if (inventory.Product.SellerId.HasValue && inventory.Product.SellerId != request.PerformedBy)
            {
                _logger.LogWarning("IDOR attempt: User {UserId} tried to adjust stock for inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.InventoryId, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            // ✅ BOLUM 1.1: Domain Method kullanımı
            var quantityBefore = inventory.Quantity;
            inventory.AdjustQuantity(request.QuantityChange);
            var quantityAfter = inventory.Quantity;

            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var stockMovement = StockMovement.Create(
                inventory.Id,
                inventory.ProductId,
                inventory.WarehouseId,
                StockMovementType.Adjustment,
                Math.Abs(request.QuantityChange),
                quantityBefore,
                quantityAfter,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                null, // fromWarehouseId
                null); // toWarehouseId

            await _context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully adjusted stock for InventoryId: {InventoryId}. Before: {Before}, After: {After}",
                request.InventoryId, quantityBefore, quantityAfter);

            // Reload with includes for AutoMapper
            inventory = await _context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId, cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.InventoryId}", cancellationToken);
            if (inventory != null)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{inventory.ProductId}_{inventory.WarehouseId}", cancellationToken);
                await _cache.RemoveAsync($"inventories_by_product_{inventory.ProductId}", cancellationToken); // Invalidate product inventories list cache
            }

            return _mapper.Map<InventoryDto>(inventory!);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while adjusting stock for InventoryId: {InventoryId}", request.InventoryId);
            throw new BusinessException("Stok güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error adjusting stock for InventoryId: {InventoryId}", request.InventoryId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

