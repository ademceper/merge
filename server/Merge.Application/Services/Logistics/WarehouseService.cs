using AutoMapper;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Inventory.Warehouse>;

namespace Merge.Application.Services.Logistics;

public class WarehouseService(IRepository warehouseRepository, IDbContext context, IMapper mapper, IUnitOfWork unitOfWork, ILogger<WarehouseService> logger) : IWarehouseService
{

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        return warehouse == null ? null : mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var warehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, cancellationToken);

        return warehouse == null ? null : mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Warehouse>().AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await context.Set<Warehouse>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Depo olusturuluyor. Code: {Code}, Name: {Name}", createDto.Code, createDto.Name);

        try
        {
            if (createDto == null)
            {
                throw new ArgumentNullException(nameof(createDto));
            }

            if (string.IsNullOrWhiteSpace(createDto.Code))
            {
                throw new ValidationException("Depo kodu boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                throw new ValidationException("Depo adı boş olamaz.");
            }

            // Check if code already exists
            var existingWarehouse = await context.Set<Warehouse>()
                .AsNoTracking()
                .AnyAsync(w => w.Code == createDto.Code, cancellationToken);

            if (existingWarehouse)
            {
                logger.LogWarning("Bu kod ile depo zaten mevcut. Code: {Code}", createDto.Code);
                throw new BusinessException($"Bu kod ile depo zaten mevcut: '{createDto.Code}'");
            }

            // Factory method kullan
            var warehouse = Warehouse.Create(
                createDto.Name,
                createDto.Code,
                createDto.Address,
                createDto.City,
                createDto.Country,
                createDto.PostalCode,
                createDto.ContactPerson,
                createDto.ContactPhone,
                createDto.ContactEmail,
                createDto.Capacity,
                createDto.Description);

            warehouse = await warehouseRepository.AddAsync(warehouse, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Depo olusturuldu. WarehouseId: {WarehouseId}, Code: {Code}", warehouse.Id, warehouse.Code);

            return mapper.Map<WarehouseDto>(warehouse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Depo olusturma hatasi. Code: {Code}, Name: {Name}", createDto?.Code, createDto?.Name);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default)
    {
        if (updateDto == null)
        {
            throw new ArgumentNullException(nameof(updateDto));
        }

        var warehouse = await warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            throw new NotFoundException("Depo", id);
        }

        // Domain method kullan - nullable değerleri mevcut değerlerle değiştir
        warehouse.UpdateDetails(
            updateDto.Name ?? warehouse.Name,
            updateDto.Address ?? warehouse.Address,
            updateDto.City ?? warehouse.City,
            updateDto.Country ?? warehouse.Country,
            updateDto.PostalCode ?? warehouse.PostalCode,
            updateDto.ContactPerson ?? warehouse.ContactPerson,
            updateDto.ContactPhone ?? warehouse.ContactPhone,
            updateDto.ContactEmail ?? warehouse.ContactEmail,
            updateDto.Capacity ?? warehouse.Capacity,
            updateDto.Description);
        
        // IsActive için domain method kullan
        if (updateDto.IsActive.HasValue)
        {
            if (updateDto.IsActive.Value && !warehouse.IsActive)
            {
                warehouse.Activate();
            }
            else if (!updateDto.IsActive.Value && warehouse.IsActive)
            {
                warehouse.Deactivate();
            }
        }

        await warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // Check if warehouse has inventory
        var hasInventory = await context.Set<Inventory>()
            .AsNoTracking()
            .AnyAsync(i => i.WarehouseId == id, cancellationToken);

        if (hasInventory)
        {
            throw new BusinessException("Envanteri olan bir depo silinemez. Önce envanteri transfer edin veya kaldırın.");
        }

        await warehouseRepository.DeleteAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // Domain method kullan
        warehouse.Activate();
        await warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // Domain method kullan
        warehouse.Deactivate();
        await warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
