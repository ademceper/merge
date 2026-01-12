using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

namespace Merge.Application.Catalog.Commands.ReserveStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Merge.Application.Interfaces.IRepository<Inventory> _inventoryRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<ReserveStockCommandHandler> _logger;
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public ReserveStockCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        Merge.Application.Interfaces.IRepository<Inventory> inventoryRepository,
        ICacheService cache,
        ILogger<ReserveStockCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reserving stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}, UserId: {UserId}",
            request.ProductId, request.WarehouseId, request.Quantity, request.OrderId, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Rezerve edilecek miktar 0'dan büyük olmalıdır.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory + StockMovement)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stokunu rezerve edebilmeli
            var product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                _logger.LogWarning("IDOR attempt: User {UserId} tried to reserve stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok rezervasyon yetkiniz yok.");
            }

            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for reservation. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.WarehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (validation entity içinde)
            var quantityBefore = inventory.Quantity;
            inventory.Reserve(request.Quantity);
            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var stockMovement = StockMovement.Create(
                inventory.Id,
                request.ProductId,
                request.WarehouseId,
                StockMovementType.Reserved,
                request.Quantity,
                quantityBefore,
                inventory.Quantity, // Total quantity doesn't change, only reserved
                request.PerformedBy,
                null, // referenceNumber
                request.OrderId, // referenceId
                $"Stock reserved for order {request.OrderId}",
                null, // fromWarehouseId
                null); // toWarehouseId

            await _context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully reserved stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                request.ProductId, request.Quantity, request.OrderId);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await _cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while reserving stock. ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok rezervasyon çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error reserving stock. ProductId: {ProductId}", request.ProductId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

