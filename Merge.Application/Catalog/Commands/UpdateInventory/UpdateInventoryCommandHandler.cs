using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Catalog.Commands.UpdateInventory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, InventoryDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateInventoryCommandHandler> _logger;
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public UpdateInventoryCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IRepository<Inventory> inventoryRepository,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdateInventoryCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InventoryDto> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating inventory Id: {InventoryId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await _context.Set<Inventory>()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found with Id: {InventoryId}", request.Id);
                throw new NotFoundException("Envanter", request.Id);
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
            if (inventory.Product.SellerId.HasValue && inventory.Product.SellerId != request.PerformedBy)
            {
                _logger.LogWarning("IDOR attempt: User {UserId} tried to update inventory {InventoryId} for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.Id, inventory.ProductId, inventory.Product.SellerId);
                throw new BusinessException("Bu envanteri güncelleme yetkiniz yok.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            inventory.UpdateStockLevels(request.MinimumStockLevel, request.MaximumStockLevel);
            inventory.UpdateUnitCost(request.UnitCost);
            inventory.UpdateLocation(request.Location);

            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            inventory = await _context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            _logger.LogInformation("Successfully updated inventory Id: {InventoryId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{request.Id}", cancellationToken);
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
            _logger.LogError(ex, "Concurrency conflict while updating inventory Id: {InventoryId}", request.Id);
            throw new BusinessException("Envanter güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating inventory Id: {InventoryId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

