using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Logistics;

public class PickPackService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PickPackService> logger) : IPickPackService
{

    public async Task<PickPackDto> CreatePickPackAsync(CreatePickPackDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Pick-pack olusturuluyor. OrderId: {OrderId}, WarehouseId: {WarehouseId}", dto.OrderId, dto.WarehouseId);

        try
        {

            var order = await context.Set<OrderEntity>()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId, cancellationToken);

            if (order == null)
            {
                throw new NotFoundException("Sipariş", dto.OrderId);
            }

            var warehouse = await context.Set<Warehouse>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId && w.IsActive, cancellationToken);

            if (warehouse == null)
            {
                throw new NotFoundException("Depo", dto.WarehouseId);
            }

            // Check if pick pack already exists for this order
            var existing = await context.Set<PickPack>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.OrderId == dto.OrderId, cancellationToken);

            if (existing != null)
            {
                logger.LogWarning("Bu siparis icin zaten bir pick pack kaydi var. OrderId: {OrderId}", dto.OrderId);
                throw new BusinessException("Bu sipariş için zaten bir pick pack kaydı var.");
            }

            var packNumber = await GeneratePackNumberAsync(cancellationToken);

            // Factory method kullan
            var pickPack = PickPack.Create(
                dto.OrderId,
                dto.WarehouseId,
                packNumber,
                dto.Notes);

            await context.Set<PickPack>().AddAsync(pickPack, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Create pick pack items from order items
            var items = new List<PickPackItem>(order.OrderItems.Count);
            foreach (var orderItem in order.OrderItems)
            {
                // Factory method kullan
                var pickPackItem = PickPackItem.Create(
                    pickPack.Id,
                    orderItem.Id,
                    orderItem.ProductId,
                    orderItem.Quantity);
                items.Add(pickPackItem);
            }

            await context.Set<PickPackItem>().AddRangeAsync(items, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var createdPickPack = await context.Set<PickPack>()
                .AsNoTracking()
                .Include(pp => pp.Order)
                .Include(pp => pp.Warehouse)
                .Include(pp => pp.PickedBy)
                .Include(pp => pp.PackedBy)
                .Include(pp => pp.Items)
                    .ThenInclude(i => i.OrderItem)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(pp => pp.Id == pickPack.Id, cancellationToken);

            logger.LogInformation("Pick-pack olusturuldu. PickPackId: {PickPackId}, PackNumber: {PackNumber}", pickPack.Id, packNumber);

            return mapper.Map<PickPackDto>(createdPickPack!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pick-pack olusturma hatasi. OrderId: {OrderId}, WarehouseId: {WarehouseId}", dto.OrderId, dto.WarehouseId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<PickPackDto?> GetPickPackByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var pickPack = await context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        return pickPack != null ? mapper.Map<PickPackDto>(pickPack) : null;
    }

    public async Task<PickPackDto?> GetPickPackByPackNumberAsync(string packNumber, CancellationToken cancellationToken = default)
    {

        var pickPack = await context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.PackNumber == packNumber, cancellationToken);

        return pickPack != null ? mapper.Map<PickPackDto>(pickPack) : null;
    }

    public async Task<IEnumerable<PickPackDto>> GetPickPacksByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var pickPacks = await context.Set<PickPack>()
            .AsNoTracking()
            .AsSplitQuery()
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

        return mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }

    public async Task<PagedResult<PickPackDto>> GetAllPickPacksAsync(string? status = null, Guid? warehouseId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        IQueryable<PickPack> query = context.Set<PickPack>()
            .AsNoTracking()
            .AsSplitQuery()
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

        var items = mapper.Map<IEnumerable<PickPackDto>>(pickPacks);

        return new PagedResult<PickPackDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<bool> UpdatePickPackStatusAsync(Guid id, UpdatePickPackStatusDto dto, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null) return false;

        if (!Enum.TryParse<PickPackStatus>(dto.Status, out var statusEnum))
        {
            throw new ValidationException("Geçersiz pick-pack durumu.");
        }

        // Domain method kullan
        pickPack.UpdateDetails(
            dto.Notes,
            dto.Weight,
            dto.Dimensions,
            dto.PackageCount);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartPickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null) return false;

        // Domain method kullan
        pickPack.StartPicking(userId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompletePickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Picking) return false;

        // Check if all items are picked
        var totalItems = await context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id, cancellationToken);

        var pickedItems = await context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPicked, cancellationToken);

        if (totalItems == 0 || pickedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler seçilmemiş.");
        }

        // Domain method kullan
        pickPack.CompletePicking();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartPackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null) return false;

        // Domain method kullan
        pickPack.StartPacking(userId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompletePackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null || pickPack.Status != PickPackStatus.Packing) return false;

        // Check if all items are packed
        var totalItems = await context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id, cancellationToken);

        var packedItems = await context.Set<PickPackItem>()
            .AsNoTracking()
            .CountAsync(i => i.PickPackId == id && i.IsPacked, cancellationToken);

        if (totalItems == 0 || packedItems < totalItems)
        {
            throw new BusinessException("Tüm kalemler paketlenmemiş.");
        }

        // Domain method kullan - CompletePacking weight, dimensions, packageCount parametreleri alıyor
        // Burada default değerler kullanıyoruz, gerçek uygulamada bu değerler dto'dan gelmeli
        pickPack.CompletePacking(weight: 0, dimensions: null, packageCount: 1);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pickPack = await context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (pickPack == null) return false;

        // Domain method kullan
        pickPack.Ship();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdatePickPackItemStatusAsync(Guid itemId, PickPackItemStatusDto dto, CancellationToken cancellationToken = default)
    {
        var item = await context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        if (item == null) return false;

        if (dto.IsPicked && !item.IsPicked)
        {
            item.MarkAsPicked();
        }

        if (dto.IsPacked && !item.IsPacked)
        {
            item.MarkAsPacked();
        }

        if (dto.Location != null)
        {
            item.UpdateLocation(dto.Location);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
    public async Task<Dictionary<string, int>> GetPickPackStatsAsync(Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = context.Set<PickPack>()
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

        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(pp => pp.Status == PickPackStatus.Pending, cancellationToken);
        var picking = await query.CountAsync(pp => pp.Status == PickPackStatus.Picking, cancellationToken);
        var packed = await query.CountAsync(pp => pp.Status == PickPackStatus.Packed, cancellationToken);
        var packing = await query.CountAsync(pp => pp.Status == PickPackStatus.Packing, cancellationToken);
        var shipped = await query.CountAsync(pp => pp.Status == PickPackStatus.Shipped, cancellationToken);
        var cancelled = await query.CountAsync(pp => pp.Status == PickPackStatus.Cancelled, cancellationToken);

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

    private async Task<string> GeneratePackNumberAsync(CancellationToken cancellationToken = default)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var existingCount = await context.Set<PickPack>()
            .AsNoTracking()
            .CountAsync(pp => pp.PackNumber.StartsWith($"PK-{date}"), cancellationToken);

        return $"PK-{date}-{(existingCount + 1):D6}";
    }
}

