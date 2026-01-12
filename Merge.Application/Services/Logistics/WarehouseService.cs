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


namespace Merge.Application.Services.Logistics;

public class WarehouseService : IWarehouseService
{
    private readonly Merge.Application.Interfaces.IRepository<Warehouse> _warehouseRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        Merge.Application.Interfaces.IRepository<Warehouse> warehouseRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return warehouse == null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return warehouse == null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var query = _context.Set<Warehouse>().AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouses = await _context.Set<Warehouse>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Depo olusturuluyor. Code: {Code}, Name: {Name}", createDto.Code, createDto.Name);

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

            // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
            // Check if code already exists
            var existingWarehouse = await _context.Set<Warehouse>()
                .AsNoTracking()
                .AnyAsync(w => w.Code == createDto.Code, cancellationToken);

            if (existingWarehouse)
            {
                _logger.LogWarning("Bu kod ile depo zaten mevcut. Code: {Code}", createDto.Code);
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

            warehouse = await _warehouseRepository.AddAsync(warehouse, cancellationToken);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Depo olusturuldu. WarehouseId: {WarehouseId}, Code: {Code}", warehouse.Id, warehouse.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<WarehouseDto>(warehouse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Depo olusturma hatasi. Code: {Code}, Name: {Name}", createDto?.Code, createDto?.Name);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default)
    {
        if (updateDto == null)
        {
            throw new ArgumentNullException(nameof(updateDto));
        }

        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
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

        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        // Check if warehouse has inventory
        var hasInventory = await _context.Set<Inventory>()
            .AsNoTracking()
            .AnyAsync(i => i.WarehouseId == id, cancellationToken);

        if (hasInventory)
        {
            throw new BusinessException("Envanteri olan bir depo silinemez. Önce envanteri transfer edin veya kaldırın.");
        }

        await _warehouseRepository.DeleteAsync(warehouse, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // Domain method kullan
        warehouse.Activate();
        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            return false;
        }

        // Domain method kullan
        warehouse.Deactivate();
        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
