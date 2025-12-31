using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;


namespace Merge.Application.Services.Catalog;

public class InventoryService : IInventoryService
{
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IRepository<StockMovement> _stockMovementRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IRepository<Inventory> inventoryRepository,
        IRepository<StockMovement> stockMovementRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _stockMovementRepository = stockMovementRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InventoryDto?> GetByIdAsync(Guid id)
    {
        var inventory = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id);

        return inventory == null ? null : _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<InventoryDto?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId)
    {
        var inventory = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

        return inventory == null ? null : _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<IEnumerable<InventoryDto>> GetByProductIdAsync(Guid productId)
    {
        var inventories = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.Warehouse.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
    }

    public async Task<IEnumerable<InventoryDto>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        var inventories = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == warehouseId)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(Guid? warehouseId = null)
    {
        var query = _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity <= i.MinimumStockLevel);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var lowStockItems = await query.ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<LowStockAlertDto>>(lowStockItems);
    }

    public async Task<StockReportDto?> GetStockReportByProductAsync(Guid productId)
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Product bilgisini database'den al (ToListAsync sonrası First() YASAK)
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var inventoryCount = await _context.Inventories
            .AsNoTracking()
            .CountAsync(i => i.ProductId == productId);

        if (inventoryCount == 0)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalQuantity = await _context.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity);

        var totalReserved = await _context.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.ReservedQuantity);

        var totalAvailable = await _context.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.AvailableQuantity);

        var totalValue = await _context.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity * i.UnitCost);

        // ✅ PERFORMANCE: Warehouse breakdown için inventory'leri yükle (AutoMapper için gerekli)
        var inventories = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        return new StockReportDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductSKU = product.SKU,
            TotalQuantity = totalQuantity,
            TotalReserved = totalReserved,
            TotalAvailable = totalAvailable,
            TotalValue = totalValue,
            WarehouseBreakdown = _mapper.Map<List<InventoryDto>>(inventories)
        };
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto createDto)
    {
        if (createDto == null)
        {
            throw new ArgumentNullException(nameof(createDto));
        }

        if (createDto.Quantity < 0)
        {
            throw new ValidationException("Miktar negatif olamaz.");
        }

        _logger.LogInformation("Creating inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}",
            createDto.ProductId, createDto.WarehouseId, createDto.Quantity);

        // Check if inventory already exists for this product-warehouse combination
        var existingInventory = await _context.Inventories
            .AsNoTracking()
            .AnyAsync(i => i.ProductId == createDto.ProductId &&
                          i.WarehouseId == createDto.WarehouseId);

        if (existingInventory)
        {
            _logger.LogWarning("Attempted to create duplicate inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                createDto.ProductId, createDto.WarehouseId);
            throw new BusinessException("Bu ürün-depo kombinasyonu için envanter zaten mevcut.");
        }

        var inventory = _mapper.Map<Inventory>(createDto);
        inventory.LastRestockedAt = DateTime.UtcNow;

        inventory = await _inventoryRepository.AddAsync(inventory);

        // Create initial stock movement
        if (createDto.Quantity > 0)
        {
            var stockMovement = new StockMovement
            {
                InventoryId = inventory.Id,
                ProductId = inventory.ProductId,
                WarehouseId = inventory.WarehouseId,
                MovementType = StockMovementType.Receipt,
                Quantity = createDto.Quantity,
                QuantityBefore = 0,
                QuantityAfter = createDto.Quantity,
                Notes = "Initial inventory creation"
            };

            await _stockMovementRepository.AddAsync(stockMovement);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        inventory = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == inventory.Id);

        _logger.LogInformation("Successfully created inventory with Id: {InventoryId} for ProductId: {ProductId}",
            inventory.Id, createDto.ProductId);

        return _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto updateDto)
    {
        if (updateDto == null)
        {
            throw new ArgumentNullException(nameof(updateDto));
        }

        _logger.LogInformation("Updating inventory Id: {InventoryId}", id);

        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
        {
            _logger.LogWarning("Inventory not found with Id: {InventoryId}", id);
            throw new NotFoundException("Envanter", id);
        }

        inventory.MinimumStockLevel = updateDto.MinimumStockLevel;
        inventory.MaximumStockLevel = updateDto.MaximumStockLevel;
        inventory.UnitCost = updateDto.UnitCost;
        inventory.Location = updateDto.Location;

        await _inventoryRepository.UpdateAsync(inventory);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        inventory = await _context.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id);

        _logger.LogInformation("Successfully updated inventory Id: {InventoryId}", id);

        return _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete inventory Id: {InventoryId}", id);

        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
        {
            _logger.LogWarning("Inventory not found for deletion with Id: {InventoryId}", id);
            return false;
        }

        if (inventory.Quantity > 0)
        {
            _logger.LogWarning("Cannot delete inventory with stock. Id: {InventoryId}, Quantity: {Quantity}",
                id, inventory.Quantity);
            throw new BusinessException("Stoklu envanter silinemez. Önce stoku sıfırlayın.");
        }

        await _inventoryRepository.DeleteAsync(inventory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted inventory Id: {InventoryId}", id);

        return true;
    }

    public async Task<InventoryDto> AdjustStockAsync(AdjustInventoryDto adjustDto, Guid userId)
    {
        if (adjustDto == null)
        {
            throw new ArgumentNullException(nameof(adjustDto));
        }

        _logger.LogInformation("Adjusting stock for InventoryId: {InventoryId}, QuantityChange: {QuantityChange}, UserId: {UserId}",
            adjustDto.InventoryId, adjustDto.QuantityChange, userId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == adjustDto.InventoryId);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for adjustment with Id: {InventoryId}", adjustDto.InventoryId);
                throw new NotFoundException("Envanter", adjustDto.InventoryId);
            }

            var quantityBefore = inventory.Quantity;
            var quantityAfter = quantityBefore + adjustDto.QuantityChange;

            if (quantityAfter < 0)
            {
                _logger.LogWarning("Stock adjustment would result in negative quantity. InventoryId: {InventoryId}, Current: {Current}, Change: {Change}",
                    adjustDto.InventoryId, quantityBefore, adjustDto.QuantityChange);
                throw new ValidationException("Stok sıfırın altına düşürülemez.");
            }

            inventory.Quantity = quantityAfter;
            if (adjustDto.QuantityChange > 0)
            {
                inventory.LastRestockedAt = DateTime.UtcNow;
            }

            await _inventoryRepository.UpdateAsync(inventory);

            // Create stock movement record
            var stockMovement = new StockMovement
            {
                InventoryId = inventory.Id,
                ProductId = inventory.ProductId,
                WarehouseId = inventory.WarehouseId,
                MovementType = StockMovementType.Adjustment,
                Quantity = adjustDto.QuantityChange,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                Notes = adjustDto.Notes,
                PerformedBy = userId
            };

            await _stockMovementRepository.AddAsync(stockMovement);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully adjusted stock for InventoryId: {InventoryId}. Before: {Before}, After: {After}",
                adjustDto.InventoryId, quantityBefore, quantityAfter);

            return _mapper.Map<InventoryDto>(inventory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Concurrency conflict while adjusting stock for InventoryId: {InventoryId}", adjustDto.InventoryId);
            throw new BusinessException("Stok güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error adjusting stock for InventoryId: {InventoryId}", adjustDto.InventoryId);
            throw;
        }
    }

    public async Task<bool> TransferStockAsync(TransferInventoryDto transferDto, Guid userId)
    {
        _logger.LogInformation("Transferring stock. ProductId: {ProductId}, From: {FromWarehouse}, To: {ToWarehouse}, Quantity: {Quantity}, UserId: {UserId}",
            transferDto.ProductId, transferDto.FromWarehouseId, transferDto.ToWarehouseId, transferDto.Quantity, userId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (transferDto.Quantity <= 0)
            {
                throw new ValidationException("Transfer miktarı 0'dan büyük olmalıdır.");
            }

            // Get source inventory
            var sourceInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == transferDto.ProductId &&
                                        i.WarehouseId == transferDto.FromWarehouseId);

            if (sourceInventory == null)
            {
                _logger.LogWarning("Source inventory not found. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    transferDto.ProductId, transferDto.FromWarehouseId);
                throw new NotFoundException("Kaynak envanter", Guid.Empty);
            }

            if (sourceInventory.AvailableQuantity < transferDto.Quantity)
            {
                _logger.LogWarning("Insufficient stock for transfer. Available: {Available}, Requested: {Requested}",
                    sourceInventory.AvailableQuantity, transferDto.Quantity);
                throw new BusinessException("Transfer için yeterli stok yok.");
            }

            // Get or create destination inventory
            var destInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == transferDto.ProductId &&
                                        i.WarehouseId == transferDto.ToWarehouseId);

            if (destInventory == null)
            {
                destInventory = new Inventory
                {
                    ProductId = transferDto.ProductId,
                    WarehouseId = transferDto.ToWarehouseId,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    MinimumStockLevel = sourceInventory.MinimumStockLevel,
                    MaximumStockLevel = sourceInventory.MaximumStockLevel,
                    UnitCost = sourceInventory.UnitCost
                };
                destInventory = await _inventoryRepository.AddAsync(destInventory);
            }

            // Update quantities
            var sourceQuantityBefore = sourceInventory.Quantity;
            sourceInventory.Quantity -= transferDto.Quantity;

            var destQuantityBefore = destInventory.Quantity;
            destInventory.Quantity += transferDto.Quantity;
            destInventory.LastRestockedAt = DateTime.UtcNow;

            await _inventoryRepository.UpdateAsync(sourceInventory);
            await _inventoryRepository.UpdateAsync(destInventory);

            // Create stock movement records
            var sourceMovement = new StockMovement
            {
                InventoryId = sourceInventory.Id,
                ProductId = transferDto.ProductId,
                WarehouseId = transferDto.FromWarehouseId,
                MovementType = StockMovementType.Transfer,
                Quantity = -transferDto.Quantity,
                QuantityBefore = sourceQuantityBefore,
                QuantityAfter = sourceInventory.Quantity,
                FromWarehouseId = transferDto.FromWarehouseId,
                ToWarehouseId = transferDto.ToWarehouseId,
                Notes = transferDto.Notes,
                PerformedBy = userId
            };

            var destMovement = new StockMovement
            {
                InventoryId = destInventory.Id,
                ProductId = transferDto.ProductId,
                WarehouseId = transferDto.ToWarehouseId,
                MovementType = StockMovementType.Transfer,
                Quantity = transferDto.Quantity,
                QuantityBefore = destQuantityBefore,
                QuantityAfter = destInventory.Quantity,
                FromWarehouseId = transferDto.FromWarehouseId,
                ToWarehouseId = transferDto.ToWarehouseId,
                Notes = transferDto.Notes,
                PerformedBy = userId
            };

            await _stockMovementRepository.AddAsync(sourceMovement);
            await _stockMovementRepository.AddAsync(destMovement);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully transferred stock. ProductId: {ProductId}, Quantity: {Quantity}, From: {FromWarehouse}, To: {ToWarehouse}",
                transferDto.ProductId, transferDto.Quantity, transferDto.FromWarehouseId, transferDto.ToWarehouseId);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Concurrency conflict during stock transfer for ProductId: {ProductId}", transferDto.ProductId);
            throw new BusinessException("Stok transfer çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error transferring stock for ProductId: {ProductId}", transferDto.ProductId);
            throw;
        }
    }

    public async Task<bool> ReserveStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Rezerve edilecek miktar 0'dan büyük olmalıdır.");
        }

        _logger.LogInformation("Reserving stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}",
            productId, warehouseId, quantity, orderId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                        i.WarehouseId == warehouseId);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for reservation. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    productId, warehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            if (inventory.AvailableQuantity < quantity)
            {
                _logger.LogWarning("Insufficient stock for reservation. Available: {Available}, Requested: {Requested}",
                    inventory.AvailableQuantity, quantity);
                throw new BusinessException("Rezerve etmek için yeterli stok yok.");
            }

            inventory.ReservedQuantity += quantity;
            await _inventoryRepository.UpdateAsync(inventory);

            // Create stock movement record
            var stockMovement = new StockMovement
            {
                InventoryId = inventory.Id,
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = StockMovementType.Reserved,
                Quantity = quantity,
                QuantityBefore = inventory.Quantity,
                QuantityAfter = inventory.Quantity, // Total quantity doesn't change, only reserved
                ReferenceId = orderId,
                Notes = "Stock reserved for order"
            };

            await _stockMovementRepository.AddAsync(stockMovement);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully reserved stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                productId, quantity, orderId);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Concurrency conflict while reserving stock. ProductId: {ProductId}", productId);
            throw new BusinessException("Stok rezervasyon çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error reserving stock. ProductId: {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> ReleaseStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Serbest bırakılacak miktar 0'dan büyük olmalıdır.");
        }

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                    i.WarehouseId == warehouseId);

        if (inventory == null)
        {
            throw new NotFoundException("Envanter", Guid.Empty);
        }

        if (inventory.ReservedQuantity < quantity)
        {
            throw new ValidationException("Rezerve edilmiş miktardan fazla stok serbest bırakılamaz.");
        }

        var quantityBefore = inventory.Quantity;
        inventory.ReservedQuantity -= quantity;
        inventory.Quantity -= quantity;

        await _inventoryRepository.UpdateAsync(inventory);

        // Create stock movement record for sale
        var stockMovement = new StockMovement
        {
            InventoryId = inventory.Id,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = StockMovementType.Sale,
            Quantity = -quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = inventory.Quantity,
            ReferenceId = orderId,
            Notes = "Stock released for order fulfillment"
        };

        await _stockMovementRepository.AddAsync(stockMovement);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetAvailableStockAsync(Guid productId, Guid? warehouseId = null)
    {
        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
        var query = _context.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        return await query.SumAsync(i => i.AvailableQuantity);
    }

    public async Task<bool> UpdateLastCountDateAsync(Guid inventoryId)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null)
        {
            return false;
        }

        inventory.LastCountedAt = DateTime.UtcNow;
        await _inventoryRepository.UpdateAsync(inventory);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
