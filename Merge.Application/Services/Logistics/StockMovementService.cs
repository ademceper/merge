using AutoMapper;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;


namespace Merge.Application.Services.Logistics;

public class StockMovementService : IStockMovementService
{
    private readonly IRepository<StockMovement> _stockMovementRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public StockMovementService(
        IRepository<StockMovement> stockMovementRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _stockMovementRepository = stockMovementRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<StockMovementDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        var movement = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return movement == null ? null : _mapper.Map<StockMovementDto>(movement);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByInventoryIdAsync(Guid inventoryId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.InventoryId == inventoryId)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.WarehouseId == warehouseId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    public async Task<IEnumerable<StockMovementDto>> GetFilteredAsync(StockMovementFilterDto filter)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        IQueryable<StockMovement> query = _context.StockMovements
            .AsNoTracking()
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
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }

    public async Task<StockMovementDto> CreateAsync(CreateStockMovementDto createDto, Guid userId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        // Get inventory
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == createDto.ProductId &&
                                    i.WarehouseId == createDto.WarehouseId);

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

        // Update inventory
        inventory.Quantity = quantityAfter;
        if (createDto.Quantity > 0)
        {
            inventory.LastRestockedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();

        // Create stock movement
        var stockMovement = new StockMovement
        {
            InventoryId = inventory.Id,
            ProductId = createDto.ProductId,
            WarehouseId = createDto.WarehouseId,
            MovementType = createDto.MovementType,
            Quantity = createDto.Quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            ReferenceNumber = createDto.ReferenceNumber,
            ReferenceId = createDto.ReferenceId,
            Notes = createDto.Notes,
            PerformedBy = userId,
            FromWarehouseId = createDto.FromWarehouseId,
            ToWarehouseId = createDto.ToWarehouseId
        };

        stockMovement = await _stockMovementRepository.AddAsync(stockMovement);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sm.IsDeleted (Global Query Filter)
        stockMovement = await _context.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == stockMovement.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StockMovementDto>(stockMovement!);
    }
}
