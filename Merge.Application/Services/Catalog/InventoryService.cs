using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Catalog;

public class InventoryService : IInventoryService
{
    private readonly Merge.Application.Interfaces.IRepository<Inventory> _inventoryRepository;
    private readonly Merge.Application.Interfaces.IRepository<StockMovement> _stockMovementRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        Merge.Application.Interfaces.IRepository<Inventory> inventoryRepository,
        Merge.Application.Interfaces.IRepository<StockMovement> stockMovementRepository,
        IDbContext context,
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InventoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return inventory == null ? null : _mapper.Map<InventoryDto>(inventory);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InventoryDto?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, cancellationToken);

        return inventory == null ? null : _mapper.Map<InventoryDto>(inventory);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ⚠️ NOT: Unbounded query riski - Bir ürün için tüm warehouse inventory'leri çekiliyor
    // Pratikte warehouse sayısı sınırlı olduğu için (genelde 10-50 arası) risk düşük
    // Ancak güvenlik için maksimum limit eklenebilir
    public async Task<IEnumerable<InventoryDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Maksimum limit (100 warehouse)
        var inventories = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.Warehouse.Name)
            .Take(100) // ✅ Güvenlik: Maksimum 100 warehouse inventory'si
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - AutoMapper internal olarak optimize eder
        return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    public async Task<PagedResult<InventoryDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == warehouseId);

        var totalCount = await query.CountAsync(cancellationToken);

        var inventories = await query
            .OrderBy(i => i.Product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryDto>
        {
            Items = _mapper.Map<List<InventoryDto>>(inventories),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    public async Task<PagedResult<LowStockAlertDto>> GetLowStockAlertsAsync(Guid? warehouseId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.Quantity <= i.MinimumStockLevel);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var lowStockItems = await query
            .OrderBy(i => i.Quantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<LowStockAlertDto>
        {
            Items = _mapper.Map<List<LowStockAlertDto>>(lowStockItems),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StockReportDto?> GetStockReportByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Product bilgisini database'den al (ToListAsync sonrası First() YASAK)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var inventoryCount = await _context.Set<Inventory>()
            .AsNoTracking()
            .CountAsync(i => i.ProductId == productId, cancellationToken);

        if (inventoryCount == 0)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalQuantity = await _context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity, cancellationToken);

        var totalReserved = await _context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.ReservedQuantity, cancellationToken);

        var totalAvailable = await _context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.AvailableQuantity, cancellationToken);

        var totalValue = await _context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity * i.UnitCost, cancellationToken);

        // ✅ PERFORMANCE: Warehouse breakdown için inventory'leri yükle (AutoMapper için gerekli)
        var inventories = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 7.1.5: Records - Positional constructor kullanımı
        return new StockReportDto(
            productId,
            product.Name,
            product.SKU,
            totalQuantity,
            totalReserved,
            totalAvailable,
            totalValue,
            _mapper.Map<List<InventoryDto>>(inventories)
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InventoryDto> CreateAsync(CreateInventoryDto createDto, CancellationToken cancellationToken = default)
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
        var existingInventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .AnyAsync(i => i.ProductId == createDto.ProductId &&
                          i.WarehouseId == createDto.WarehouseId, cancellationToken);

        if (existingInventory)
        {
            _logger.LogWarning("Attempted to create duplicate inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                createDto.ProductId, createDto.WarehouseId);
            throw new BusinessException("Bu ürün-depo kombinasyonu için envanter zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Factory Method kullanımı
        var inventory = Inventory.Create(
            createDto.ProductId,
            createDto.WarehouseId,
            createDto.Quantity,
            createDto.MinimumStockLevel,
            createDto.MaximumStockLevel,
            createDto.UnitCost,
            createDto.Location);

        inventory = await _inventoryRepository.AddAsync(inventory, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // Create initial stock movement
        if (createDto.Quantity > 0)
        {
            var stockMovement = StockMovement.Create(
                inventory.Id,
                inventory.ProductId,
                inventory.WarehouseId,
                StockMovementType.Receipt,
                createDto.Quantity,
                0, // quantityBefore
                createDto.Quantity, // quantityAfter
                null, // performedBy
                null, // referenceNumber
                null, // referenceId
                "Initial inventory creation",
                null, // fromWarehouseId
                null); // toWarehouseId

            await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        var reloadedInventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == inventory.Id, cancellationToken);

        if (reloadedInventory == null)
        {
            _logger.LogWarning("Inventory {InventoryId} not found after creation", inventory.Id);
            return _mapper.Map<InventoryDto>(inventory);
        }

        _logger.LogInformation("Successfully created inventory with Id: {InventoryId} for ProductId: {ProductId}",
            reloadedInventory.Id, createDto.ProductId);

        return _mapper.Map<InventoryDto>(reloadedInventory);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto updateDto, CancellationToken cancellationToken = default)
    {
        if (updateDto == null)
        {
            throw new ArgumentNullException(nameof(updateDto));
        }

        _logger.LogInformation("Updating inventory Id: {InventoryId}", id);

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var inventory = await _inventoryRepository.GetByIdAsync(id, cancellationToken);
        if (inventory == null)
        {
            _logger.LogWarning("Inventory not found with Id: {InventoryId}", id);
            throw new NotFoundException("Envanter", id);
        }

        // ✅ BOLUM 1.1: Domain Method kullanımı
        if (updateDto.MinimumStockLevel.HasValue || updateDto.MaximumStockLevel.HasValue)
        {
            var minLevel = updateDto.MinimumStockLevel ?? inventory.MinimumStockLevel;
            var maxLevel = updateDto.MaximumStockLevel ?? inventory.MaximumStockLevel;
            inventory.UpdateStockLevels(minLevel, maxLevel);
        }
        
        if (updateDto.UnitCost.HasValue)
        {
            inventory.UpdateUnitCost(updateDto.UnitCost.Value);
        }
        
        if (updateDto.Location != null)
        {
            inventory.UpdateLocation(updateDto.Location);
        }

        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        inventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        _logger.LogInformation("Successfully updated inventory Id: {InventoryId}", id);

        return _mapper.Map<InventoryDto>(inventory);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete inventory Id: {InventoryId}", id);

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var inventory = await _inventoryRepository.GetByIdAsync(id, cancellationToken);
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

        await _inventoryRepository.DeleteAsync(inventory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted inventory Id: {InventoryId}", id);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InventoryDto> AdjustStockAsync(AdjustInventoryDto adjustDto, Guid userId, CancellationToken cancellationToken = default)
    {
        if (adjustDto == null)
        {
            throw new ArgumentNullException(nameof(adjustDto));
        }

        _logger.LogInformation("Adjusting stock for InventoryId: {InventoryId}, QuantityChange: {QuantityChange}, UserId: {UserId}",
            adjustDto.InventoryId, adjustDto.QuantityChange, userId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await _context.Set<Inventory>()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == adjustDto.InventoryId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for adjustment with Id: {InventoryId}", adjustDto.InventoryId);
                throw new NotFoundException("Envanter", adjustDto.InventoryId);
            }

            // ✅ BOLUM 1.1: Domain Method kullanımı
            var quantityBefore = inventory.Quantity;
            inventory.AdjustQuantity(adjustDto.QuantityChange);
            var quantityAfter = inventory.Quantity;

            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var stockMovement = StockMovement.Create(
                inventory.Id,
                inventory.ProductId,
                inventory.WarehouseId,
                StockMovementType.Adjustment,
                Math.Abs(adjustDto.QuantityChange),
                quantityBefore,
                quantityAfter,
                userId,
                null, // referenceNumber
                null, // referenceId
                adjustDto.Notes,
                null, // fromWarehouseId
                null); // toWarehouseId

            await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully adjusted stock for InventoryId: {InventoryId}. Before: {Before}, After: {After}",
                adjustDto.InventoryId, quantityBefore, quantityAfter);

            return _mapper.Map<InventoryDto>(inventory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while adjusting stock for InventoryId: {InventoryId}", adjustDto.InventoryId);
            throw new BusinessException("Stok güncelleme çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error adjusting stock for InventoryId: {InventoryId}", adjustDto.InventoryId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> TransferStockAsync(TransferInventoryDto transferDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transferring stock. ProductId: {ProductId}, From: {FromWarehouse}, To: {ToWarehouse}, Quantity: {Quantity}, UserId: {UserId}",
            transferDto.ProductId, transferDto.FromWarehouseId, transferDto.ToWarehouseId, transferDto.Quantity, userId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (transferDto.Quantity <= 0)
            {
                throw new ValidationException("Transfer miktarı 0'dan büyük olmalıdır.");
            }

            // Get source inventory
            var sourceInventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == transferDto.ProductId &&
                                        i.WarehouseId == transferDto.FromWarehouseId, cancellationToken);

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
            var destInventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == transferDto.ProductId &&
                                        i.WarehouseId == transferDto.ToWarehouseId, cancellationToken);

            if (destInventory == null)
            {
                // ✅ BOLUM 1.1: Factory Method kullanımı
                destInventory = Inventory.Create(
                    transferDto.ProductId,
                    transferDto.ToWarehouseId,
                    0,
                    sourceInventory.MinimumStockLevel,
                    sourceInventory.MaximumStockLevel,
                    sourceInventory.UnitCost,
                    null);
                destInventory = await _inventoryRepository.AddAsync(destInventory, cancellationToken);
            }

            // ✅ BOLUM 1.1: Domain Method kullanımı
            var sourceQuantityBefore = sourceInventory.Quantity;
            sourceInventory.AdjustQuantity(-transferDto.Quantity);

            var destQuantityBefore = destInventory.Quantity;
            destInventory.AdjustQuantity(transferDto.Quantity);

            await _inventoryRepository.UpdateAsync(sourceInventory, cancellationToken);
            await _inventoryRepository.UpdateAsync(destInventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var sourceMovement = StockMovement.Create(
                sourceInventory.Id,
                transferDto.ProductId,
                transferDto.FromWarehouseId,
                StockMovementType.Transfer,
                transferDto.Quantity, // Absolute value
                sourceQuantityBefore,
                sourceInventory.Quantity,
                userId,
                null, // referenceNumber
                null, // referenceId
                transferDto.Notes,
                transferDto.FromWarehouseId,
                transferDto.ToWarehouseId);

            var destMovement = StockMovement.Create(
                destInventory.Id,
                transferDto.ProductId,
                transferDto.ToWarehouseId,
                StockMovementType.Transfer,
                transferDto.Quantity,
                destQuantityBefore,
                destInventory.Quantity,
                userId,
                null, // referenceNumber
                null, // referenceId
                transferDto.Notes,
                transferDto.FromWarehouseId,
                transferDto.ToWarehouseId);

            await _stockMovementRepository.AddAsync(sourceMovement, cancellationToken);
            await _stockMovementRepository.AddAsync(destMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully transferred stock. ProductId: {ProductId}, Quantity: {Quantity}, From: {FromWarehouse}, To: {ToWarehouse}",
                transferDto.ProductId, transferDto.Quantity, transferDto.FromWarehouseId, transferDto.ToWarehouseId);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict during stock transfer for ProductId: {ProductId}", transferDto.ProductId);
            throw new BusinessException("Stok transfer çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error transferring stock for ProductId: {ProductId}", transferDto.ProductId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ReserveStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Rezerve edilecek miktar 0'dan büyük olmalıdır.");
        }

        _logger.LogInformation("Reserving stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}",
            productId, warehouseId, quantity, orderId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                        i.WarehouseId == warehouseId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for reservation. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    productId, warehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            // ✅ BOLUM 1.1: Domain Method kullanımı (validation entity içinde)
            var quantityBefore = inventory.Quantity;
            inventory.Reserve(quantity);
            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var stockMovement = StockMovement.Create(
                inventory.Id,
                productId,
                warehouseId,
                StockMovementType.Reserved,
                quantity,
                quantityBefore,
                inventory.Quantity, // Total quantity doesn't change, only reserved
                null, // performedBy
                null, // referenceNumber
                orderId, // referenceId
                "Stock reserved for order",
                null, // fromWarehouseId
                null); // toWarehouseId

            await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);

            // Save changes with concurrency check
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully reserved stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                productId, quantity, orderId);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while reserving stock. ProductId: {ProductId}", productId);
            throw new BusinessException("Stok rezervasyon çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error reserving stock. ProductId: {ProductId}", productId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ReleaseStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Serbest bırakılacak miktar 0'dan büyük olmalıdır.");
        }

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
        var inventory = await _context.Set<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                    i.WarehouseId == warehouseId, cancellationToken);

        if (inventory == null)
        {
            throw new NotFoundException("Envanter", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Domain Method kullanımı (validation entity içinde)
        var quantityBefore = inventory.Quantity;
        inventory.ReleaseAndReduce(quantity);

        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var stockMovement = StockMovement.Create(
            inventory.Id,
            productId,
            warehouseId,
            StockMovementType.Sale,
            quantity, // Absolute value
            quantityBefore,
            inventory.Quantity,
            null, // performedBy
            null, // referenceNumber
            orderId, // referenceId
            "Stock released for order fulfillment",
            null, // fromWarehouseId
            null); // toWarehouseId

        await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> GetAvailableStockAsync(Guid productId, Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<Inventory>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId);

        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }

        return await query.SumAsync(i => i.AvailableQuantity, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateLastCountDateAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId, cancellationToken);
        if (inventory == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Domain Method kullanımı
        inventory.UpdateLastCountedDate();
        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
