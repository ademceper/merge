using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;


namespace Merge.Application.Services.Logistics;

public class PickPackService : IPickPackService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PickPackService> _logger;

    public PickPackService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PickPackService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<PickPackDto> CreatePickPackAsync(CreatePickPackDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pick-pack olusturuluyor. OrderId: {OrderId}, WarehouseId: {WarehouseId}", dto.OrderId, dto.WarehouseId);

        try
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
            var order = await _context.Set<OrderEntity>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId, cancellationToken);

            if (order == null)
            {
                throw new NotFoundException("Sipariş", dto.OrderId);
            }

            // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
            var warehouse = await _context.Set<Warehouse>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId && w.IsActive, cancellationToken);

            if (warehouse == null)
            {
                throw new NotFoundException("Depo", dto.WarehouseId);
            }

            // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
            // Check if pick pack already exists for this order
            var existing = await _context.Set<PickPack>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.OrderId == dto.OrderId, cancellationToken);

            if (existing != null)
            {
                _logger.LogWarning("Bu siparis icin zaten bir pick pack kaydi var. OrderId: {OrderId}", dto.OrderId);
                throw new BusinessException("Bu sipariş için zaten bir pick pack kaydı var.");
            }

            var packNumber = await GeneratePackNumberAsync(cancellationToken);

            var pickPack = new PickPack
            {
                OrderId = dto.OrderId,
                WarehouseId = dto.WarehouseId,
                PackNumber = packNumber,
                Status = PickPackStatus.Pending,
                Notes = dto.Notes
            };

            await _context.Set<PickPack>().AddAsync(pickPack, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create pick pack items from order items
            // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
            var items = new List<PickPackItem>(order.OrderItems.Count);
            foreach (var orderItem in order.OrderItems)
            {
                var pickPackItem = new PickPackItem
                {
                    PickPackId = pickPack.Id,
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    IsPicked = false,
                    IsPacked = false
                };
                items.Add(pickPackItem);
            }

            await _context.Set<PickPackItem>().AddRangeAsync(items, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
            var createdPickPack = await _context.Set<PickPack>()
                .AsNoTracking()
                .Include(pp => pp.Order)
                .Include(pp => pp.Warehouse)
                .Include(pp => pp.PickedBy)
                .Include(pp => pp.PackedBy)
                .Include(pp => pp.Items)
                    .ThenInclude(i => i.OrderItem)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(pp => pp.Id == pickPack.Id, cancellationToken);

            _logger.LogInformation("Pick-pack olusturuldu. PickPackId: {PickPackId}, PackNumber: {PackNumber}", pickPack.Id, packNumber);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PickPackDto>(createdPickPack!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pick-pack olusturma hatasi. OrderId: {OrderId}, WarehouseId: {WarehouseId}", dto.OrderId, dto.WarehouseId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PickPackDto?> GetPickPackByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return pickPack != null ? _mapper.Map<PickPackDto>(pickPack) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PickPackDto?> GetPickPackByPackNumberAsync(string packNumber, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.PackNumber == packNumber, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return pickPack != null ? _mapper.Map<PickPackDto>(pickPack) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<PickPackDto>> GetPickPacksByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var pickPacks = await _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .Where(pp => pp.OrderId == orderId)
            .OrderByDescending(pp => pp.CreatedAt)
            .Take(50) // ✅ Güvenlik: Maksimum 50 pick-pack
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<PickPackDto>> GetAllPickPacksAsync(string? status = null, Guid? warehouseId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        IQueryable<PickPack> query = _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product);

        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<PickPackStatus>(status, out var statusEnum))
            {
                throw new ValidationException("Geçersiz pick-pack durumu.");
            }
            query = query.Where(pp => pp.Status == statusEnum);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(pp => pp.WarehouseId == warehouseId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pickPacks = await query
            .OrderByDescending(pp => pp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var items = _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);

        return new PagedResult<PickPackDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdatePickPackStatusAsync(Guid id, UpdatePickPackStatusDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null) return false;

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!Enum.TryParse<PickPackStatus>(dto.Status, out var statusEnum))
        {
            throw new ValidationException("Geçersiz pick-pack durumu.");
        }

        pickPack.Status = statusEnum;
        if (dto.Notes != null)
            pickPack.Notes = dto.Notes;
        if (dto.Weight.HasValue)
            pickPack.Weight = dto.Weight.Value;
        if (dto.Dimensions != null)
            pickPack.Dimensions = dto.Dimensions;
        if (dto.PackageCount.HasValue)
            pickPack.PackageCount = dto.PackageCount.Value;

        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> StartPickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Pending) return false;

        pickPack.Status = PickPackStatus.Picking;
        pickPack.PickedByUserId = userId;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompletePickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Picking) return false;

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // Check if all items are picked
        var totalItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id, cancellationToken);

        var pickedItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPicked, cancellationToken);

        if (totalItems == 0 || pickedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler seçilmemiş.");
        }

        pickPack.Status = PickPackStatus.Packed;
        pickPack.PickedByUserId = userId;
        pickPack.PickedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> StartPackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || (pickPack.Status != PickPackStatus.Packed && pickPack.Status != PickPackStatus.Picking)) return false;

        pickPack.Status = PickPackStatus.Packing;
        pickPack.PackedByUserId = userId;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompletePackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Packing) return false;

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // Check if all items are packed
        var totalItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id, cancellationToken);

        var packedItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPacked, cancellationToken);

        if (totalItems == 0 || packedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler paketlenmemiş.");
        }

        pickPack.Status = PickPackStatus.Shipped;
        pickPack.PackedByUserId = userId;
        pickPack.PackedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Shipped) return false;

        pickPack.ShippedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdatePickPackItemStatusAsync(Guid itemId, PickPackItemStatusDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var item = await _context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        if (item == null) return false;

        if (dto.IsPicked && !item.IsPicked)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            item.MarkAsPicked();
        }

        if (dto.IsPacked && !item.IsPacked)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            item.MarkAsPacked();
        }

        if (dto.Location != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            item.UpdateLocation(dto.Location);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
    public async Task<Dictionary<string, int>> GetPickPackStatsAsync(Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        var query = _context.Set<PickPack>()
            .AsNoTracking();

        if (warehouseId.HasValue)
        {
            query = query.Where(pp => pp.WarehouseId == warehouseId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(pp => pp.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(pp => pp.CreatedAt <= endDate.Value);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(pp => pp.Status == PickPackStatus.Pending, cancellationToken);
        var picking = await query.CountAsync(pp => pp.Status == PickPackStatus.Picking, cancellationToken);
        var packed = await query.CountAsync(pp => pp.Status == PickPackStatus.Packed, cancellationToken);
        var packing = await query.CountAsync(pp => pp.Status == PickPackStatus.Packing, cancellationToken);
        var shipped = await query.CountAsync(pp => pp.Status == PickPackStatus.Shipped, cancellationToken);
        var cancelled = await query.CountAsync(pp => pp.Status == PickPackStatus.Cancelled, cancellationToken);

        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece Dictionary oluşturma)
        return new Dictionary<string, int>
        {
            { "Total", total },
            { "Pending", pending },
            { "Picking", picking },
            { "Packed", packed },
            { "Packing", packing },
            { "Shipped", shipped },
            { "Cancelled", cancelled }
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<string> GeneratePackNumberAsync(CancellationToken cancellationToken = default)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        var existingCount = await _context.Set<PickPack>()
            .AsNoTracking()
            .CountAsync(pp => pp.PackNumber.StartsWith($"PK-{date}"), cancellationToken);

        return $"PK-{date}-{(existingCount + 1):D6}";
    }
}

