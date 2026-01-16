using AutoMapper;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Inventory.StockMovement>;


namespace Merge.Application.Services.Logistics;

public class StockMovementService : IStockMovementService
{
    private readonly IRepository _stockMovementRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(
        IRepository stockMovementRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<StockMovementService> logger)
    {
        _stockMovementRepository = stockMovementRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StockMovementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var movement = await _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return movement == null ? null : _mapper.Map<StockMovementDto>(movement);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<StockMovementDto>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var movements = await _context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.InventoryId == inventoryId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Take(100) // ✅ Güvenlik: Maksimum 100 hareket
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<StockMovementDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.ProductId == productId);

        var totalCount = await query.CountAsync(cancellationToken);

        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var items = _mapper.Map<IEnumerable<StockMovementDto>>(movements);

        return new PagedResult<StockMovementDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<StockMovementDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.WarehouseId == warehouseId);

        var totalCount = await query.CountAsync(cancellationToken);

        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var items = _mapper.Map<IEnumerable<StockMovementDto>>(movements);

        return new PagedResult<StockMovementDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<StockMovementDto>> GetFilteredAsync(StockMovementFilterDto filter, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // Init-only property'ler için yeni filter oluştur
        var pageSize = filter.PageSize > 100 ? 100 : filter.PageSize;
        var page = filter.Page < 1 ? 1 : filter.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        IQueryable<StockMovement> query = _context.Set<StockMovement>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse);

        if (filter.ProductId.HasValue)
        {
            query = query.Where(sm => sm.ProductId == filter.ProductId.Value);
        }

        if (filter.WarehouseId.HasValue)
        {
            query = query.Where(sm => sm.WarehouseId == filter.WarehouseId.Value);
        }

        if (filter.MovementType.HasValue)
        {
            query = query.Where(sm => sm.MovementType == filter.MovementType.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt <= filter.EndDate.Value);
        }

        var movements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<StockMovementDto> CreateAsync(CreateStockMovementDto createDto, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory update + StockMovement create)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}",
                createDto.ProductId, createDto.WarehouseId, createDto.Quantity);

            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
            // Get inventory
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == createDto.ProductId &&
                                        i.WarehouseId == createDto.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            var quantityBefore = inventory.Quantity;
            var quantityAfter = quantityBefore + createDto.Quantity;

            if (quantityAfter < 0)
            {
                throw new ValidationException("Stok miktarı negatif olamaz.");
            }

            // Update inventory using domain method
            var quantityChange = quantityAfter - inventory.Quantity;
            inventory.AdjustQuantity(quantityChange);

            // Factory method kullan
            var stockMovement = StockMovement.Create(
                inventory.Id,
                createDto.ProductId,
                createDto.WarehouseId,
                createDto.MovementType,
                createDto.Quantity,
                quantityBefore,
                quantityAfter,
                userId,
                createDto.ReferenceNumber,
                createDto.ReferenceId,
                createDto.Notes,
                createDto.FromWarehouseId,
                createDto.ToWarehouseId);

            stockMovement = await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Stock movement created successfully. StockMovementId: {StockMovementId}",
                stockMovement.Id);

            stockMovement = await _context.Set<StockMovement>()
                .AsNoTracking()
                .Include(sm => sm.Product)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .FirstOrDefaultAsync(sm => sm.Id == stockMovement.Id, cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<StockMovementDto>(stockMovement!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                createDto.ProductId, createDto.WarehouseId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }
}
