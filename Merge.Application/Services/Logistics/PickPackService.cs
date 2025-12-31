using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;


namespace Merge.Application.Services.Logistics;

public class PickPackService : IPickPackService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PickPackService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PickPackDto> CreatePickPackAsync(CreatePickPackDto dto)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId && w.IsActive);

        if (warehouse == null)
        {
            throw new NotFoundException("Depo", dto.WarehouseId);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        // Check if pick pack already exists for this order
        var existing = await _context.Set<PickPack>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.OrderId == dto.OrderId);

        if (existing != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir pick pack kaydı var.");
        }

        var packNumber = await GeneratePackNumberAsync();

        var pickPack = new PickPack
        {
            OrderId = dto.OrderId,
            WarehouseId = dto.WarehouseId,
            PackNumber = packNumber,
            Status = "Pending",
            Notes = dto.Notes
        };

        await _context.Set<PickPack>().AddAsync(pickPack);
        await _unitOfWork.SaveChangesAsync();

        // Create pick pack items from order items
        var items = new List<PickPackItem>();
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

        await _context.Set<PickPackItem>().AddRangeAsync(items);
        await _unitOfWork.SaveChangesAsync();

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
            .FirstOrDefaultAsync(pp => pp.Id == pickPack.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PickPackDto>(createdPickPack!);
    }

    public async Task<PickPackDto?> GetPickPackByIdAsync(Guid id)
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
            .FirstOrDefaultAsync(pp => pp.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return pickPack != null ? _mapper.Map<PickPackDto>(pickPack) : null;
    }

    public async Task<PickPackDto?> GetPickPackByPackNumberAsync(string packNumber)
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
            .FirstOrDefaultAsync(pp => pp.PackNumber == packNumber);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return pickPack != null ? _mapper.Map<PickPackDto>(pickPack) : null;
    }

    public async Task<IEnumerable<PickPackDto>> GetPickPacksByOrderIdAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }

    public async Task<IEnumerable<PickPackDto>> GetAllPickPacksAsync(string? status = null, Guid? warehouseId = null, int page = 1, int pageSize = 20)
    {
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
            query = query.Where(pp => pp.Status == status);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(pp => pp.WarehouseId == warehouseId.Value);
        }

        var pickPacks = await query
            .OrderByDescending(pp => pp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }

    public async Task<bool> UpdatePickPackStatusAsync(Guid id, UpdatePickPackStatusDto dto, Guid? userId = null)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null) return false;

        pickPack.Status = dto.Status;
        if (dto.Notes != null)
            pickPack.Notes = dto.Notes;
        if (dto.Weight.HasValue)
            pickPack.Weight = dto.Weight.Value;
        if (dto.Dimensions != null)
            pickPack.Dimensions = dto.Dimensions;
        if (dto.PackageCount.HasValue)
            pickPack.PackageCount = dto.PackageCount.Value;

        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> StartPickingAsync(Guid id, Guid userId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null || pickPack.Status != "Pending") return false;

        pickPack.Status = "Picking";
        pickPack.PickedByUserId = userId;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompletePickingAsync(Guid id, Guid userId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null || pickPack.Status != "Picking") return false;

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // Check if all items are picked
        var totalItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id);

        var pickedItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPicked);

        if (totalItems == 0 || pickedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler seçilmemiş.");
        }

        pickPack.Status = "Packed";
        pickPack.PickedByUserId = userId;
        pickPack.PickedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> StartPackingAsync(Guid id, Guid userId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null || (pickPack.Status != "Packed" && pickPack.Status != "Picking")) return false;

        pickPack.Status = "Packing";
        pickPack.PackedByUserId = userId;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompletePackingAsync(Guid id, Guid userId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null || pickPack.Status != "Packing") return false;

        // ✅ PERFORMANCE: Database'de kontrol et (memory'de işlem YASAK)
        // Check if all items are packed
        var totalItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id);

        var packedItems = await _context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPacked);

        if (totalItems == 0 || packedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler paketlenmemiş.");
        }

        pickPack.Status = "Shipped";
        pickPack.PackedByUserId = userId;
        pickPack.PackedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsShippedAsync(Guid id)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !pp.IsDeleted (Global Query Filter)
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (pickPack == null || pickPack.Status != "Shipped") return false;

        pickPack.ShippedAt = DateTime.UtcNow;
        pickPack.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePickPackItemStatusAsync(Guid itemId, PickPackItemStatusDto dto)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var item = await _context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null) return false;

        if (dto.IsPicked && !item.IsPicked)
        {
            item.IsPicked = true;
            item.PickedAt = DateTime.UtcNow;
        }

        if (dto.IsPacked && !item.IsPacked)
        {
            item.IsPacked = true;
            item.PackedAt = DateTime.UtcNow;
        }

        if (dto.Location != null)
        {
            item.Location = dto.Location;
        }

        item.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Dictionary<string, int>> GetPickPackStatsAsync(Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null)
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
        var total = await query.CountAsync();
        var pending = await query.CountAsync(pp => pp.Status == "Pending");
        var picking = await query.CountAsync(pp => pp.Status == "Picking");
        var packed = await query.CountAsync(pp => pp.Status == "Packed");
        var packing = await query.CountAsync(pp => pp.Status == "Packing");
        var shipped = await query.CountAsync(pp => pp.Status == "Shipped");
        var cancelled = await query.CountAsync(pp => pp.Status == "Cancelled");

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

    private async Task<string> GeneratePackNumberAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pp.IsDeleted (Global Query Filter)
        var existingCount = await _context.Set<PickPack>()
            .AsNoTracking()
            .CountAsync(pp => pp.PackNumber.StartsWith($"PK-{date}"));

        return $"PK-{date}-{(existingCount + 1):D6}";
    }
}

