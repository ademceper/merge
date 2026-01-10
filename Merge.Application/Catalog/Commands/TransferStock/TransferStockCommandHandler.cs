using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Catalog.Commands.TransferStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<TransferStockCommandHandler> _logger;
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public TransferStockCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IRepository<Inventory> inventoryRepository,
        ICacheService cache,
        ILogger<TransferStockCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transferring stock. ProductId: {ProductId}, From: {FromWarehouse}, To: {ToWarehouse}, Quantity: {Quantity}, UserId: {UserId}",
            request.ProductId, request.FromWarehouseId, request.ToWarehouseId, request.Quantity, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Transfer miktarı 0'dan büyük olmalıdır.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (2 Inventory + 2 StockMovement)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini transfer edebilmeli
            var product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                _logger.LogWarning("IDOR attempt: User {UserId} tried to transfer stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok transfer yetkiniz yok.");
            }

            // Get source inventory
            var sourceInventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.FromWarehouseId, cancellationToken);

            if (sourceInventory == null)
            {
                _logger.LogWarning("Source inventory not found. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.FromWarehouseId);
                throw new NotFoundException("Kaynak envanter", Guid.Empty);
            }

            if (sourceInventory.AvailableQuantity < request.Quantity)
            {
                _logger.LogWarning("Insufficient stock for transfer. Available: {Available}, Requested: {Requested}",
                    sourceInventory.AvailableQuantity, request.Quantity);
                throw new BusinessException("Transfer için yeterli stok yok.");
            }

            // Get or create destination inventory
            var destInventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.ToWarehouseId, cancellationToken);

            if (destInventory == null)
            {
                // ✅ BOLUM 1.1: Factory Method kullanımı
                destInventory = Inventory.Create(
                    request.ProductId,
                    request.ToWarehouseId,
                    0,
                    sourceInventory.MinimumStockLevel,
                    sourceInventory.MaximumStockLevel,
                    sourceInventory.UnitCost,
                    null);
                destInventory = await _inventoryRepository.AddAsync(destInventory, cancellationToken);
            }

            // ✅ BOLUM 1.1: Domain Method kullanımı
            var sourceQuantityBefore = sourceInventory.Quantity;
            sourceInventory.AdjustQuantity(-request.Quantity);

            var destQuantityBefore = destInventory.Quantity;
            destInventory.AdjustQuantity(request.Quantity);

            await _inventoryRepository.UpdateAsync(sourceInventory, cancellationToken);
            await _inventoryRepository.UpdateAsync(destInventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var sourceMovement = StockMovement.Create(
                sourceInventory.Id,
                request.ProductId,
                request.FromWarehouseId,
                StockMovementType.Transfer,
                request.Quantity, // Absolute value, negative is handled by MovementType
                sourceQuantityBefore,
                sourceInventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                request.FromWarehouseId,
                request.ToWarehouseId);

            var destMovement = StockMovement.Create(
                destInventory.Id,
                request.ProductId,
                request.ToWarehouseId,
                StockMovementType.Transfer,
                request.Quantity,
                destQuantityBefore,
                destInventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                null, // referenceId
                request.Notes,
                request.FromWarehouseId,
                request.ToWarehouseId);

            await _context.Set<StockMovement>().AddAsync(sourceMovement, cancellationToken);
            await _context.Set<StockMovement>().AddAsync(destMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully transferred stock. ProductId: {ProductId}, Quantity: {Quantity}, From: {FromWarehouse}, To: {ToWarehouse}",
                request.ProductId, request.Quantity, request.FromWarehouseId, request.ToWarehouseId);

            // ✅ BOLUM 10.2: Cache invalidation - Both source and destination inventory caches
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.FromWarehouseId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.ToWarehouseId}", cancellationToken);
            await _cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict during stock transfer for ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok transfer çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error transferring stock for ProductId: {ProductId}", request.ProductId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

