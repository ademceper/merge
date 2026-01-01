using AutoMapper;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;


namespace Merge.Application.Services.Logistics;

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IRepository<Warehouse> warehouseRepository,
        ApplicationDbContext context,
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

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return warehouse == null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto?> GetByCodeAsync(string code)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return warehouse == null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive = false)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var query = _context.Warehouses.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto createDto)
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
        var existingWarehouse = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.Code == createDto.Code);

        if (existingWarehouse)
        {
            throw new BusinessException($"Bu kod ile depo zaten mevcut: '{createDto.Code}'");
        }

        var warehouse = _mapper.Map<Warehouse>(createDto);
        warehouse.IsActive = true;

        warehouse = await _warehouseRepository.AddAsync(warehouse);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateDto)
    {
        if (updateDto == null)
        {
            throw new ArgumentNullException(nameof(updateDto));
        }

        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
        {
            throw new NotFoundException("Depo", id);
        }

        warehouse.Name = updateDto.Name;
        warehouse.Address = updateDto.Address;
        warehouse.City = updateDto.City;
        warehouse.Country = updateDto.Country;
        warehouse.PostalCode = updateDto.PostalCode;
        warehouse.ContactPerson = updateDto.ContactPerson;
        warehouse.ContactPhone = updateDto.ContactPhone;
        warehouse.ContactEmail = updateDto.ContactEmail;
        warehouse.Capacity = updateDto.Capacity;
        warehouse.IsActive = updateDto.IsActive;
        warehouse.Description = updateDto.Description;

        await _warehouseRepository.UpdateAsync(warehouse);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        // Check if warehouse has inventory
        var hasInventory = await _context.Inventories
            .AsNoTracking()
            .AnyAsync(i => i.WarehouseId == id);

        if (hasInventory)
        {
            throw new BusinessException("Envanteri olan bir depo silinemez. Önce envanteri transfer edin veya kaldırın.");
        }

        await _warehouseRepository.DeleteAsync(warehouse);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return false;
        }

        warehouse.IsActive = true;
        await _warehouseRepository.UpdateAsync(warehouse);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return false;
        }

        warehouse.IsActive = false;
        await _warehouseRepository.UpdateAsync(warehouse);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
