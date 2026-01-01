using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;


namespace Merge.Application.Services.Logistics;

public class ShippingAddressService : IShippingAddressService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ShippingAddressService> _logger;

    public ShippingAddressService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShippingAddressService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingAddressDto> CreateShippingAddressAsync(Guid userId, CreateShippingAddressDto dto)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // If this is default, unset other default addresses
        if (dto.IsDefault)
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var existingDefault = await _context.Set<ShippingAddress>()
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var existingAddr in existingDefault)
            {
                existingAddr.IsDefault = false;
            }
        }

        var address = new ShippingAddress
        {
            UserId = userId,
            Label = dto.Label,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefault = dto.IsDefault,
            IsActive = true,
            Instructions = dto.Instructions
        };

        await _context.Set<ShippingAddress>().AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingAddressDto>(address);
    }

    public async Task<ShippingAddressDto?> GetShippingAddressByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }

    public async Task<IEnumerable<ShippingAddressDto>> GetUserShippingAddressesAsync(Guid userId, bool? isActive = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var query = _context.Set<ShippingAddress>()
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        var addresses = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ShippingAddressDto>>(addresses);
    }

    public async Task<ShippingAddressDto?> GetDefaultShippingAddressAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.IsActive);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }

    public async Task<bool> UpdateShippingAddressAsync(Guid id, UpdateShippingAddressDto dto)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (address == null) return false;

        if (!string.IsNullOrEmpty(dto.Label))
        {
            address.Label = dto.Label;
        }

        if (!string.IsNullOrEmpty(dto.FirstName))
        {
            address.FirstName = dto.FirstName;
        }

        if (!string.IsNullOrEmpty(dto.LastName))
        {
            address.LastName = dto.LastName;
        }

        if (dto.Phone != null)
        {
            address.Phone = dto.Phone;
        }

        if (!string.IsNullOrEmpty(dto.AddressLine1))
        {
            address.AddressLine1 = dto.AddressLine1;
        }

        if (dto.AddressLine2 != null)
        {
            address.AddressLine2 = dto.AddressLine2;
        }

        if (!string.IsNullOrEmpty(dto.City))
        {
            address.City = dto.City;
        }

        if (dto.State != null)
        {
            address.State = dto.State;
        }

        if (dto.PostalCode != null)
        {
            address.PostalCode = dto.PostalCode;
        }

        if (!string.IsNullOrEmpty(dto.Country))
        {
            address.Country = dto.Country;
        }

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            // Unset other default addresses
            var existingDefault = await _context.Set<ShippingAddress>()
                .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != id)
                .ToListAsync();

            foreach (var a in existingDefault)
            {
                a.IsDefault = false;
            }

            address.IsDefault = true;
        }
        else if (dto.IsDefault.HasValue && !dto.IsDefault.Value)
        {
            address.IsDefault = false;
        }

        if (dto.IsActive.HasValue)
        {
            address.IsActive = dto.IsActive.Value;
        }

        if (dto.Instructions != null)
        {
            address.Instructions = dto.Instructions;
        }

        address.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteShippingAddressAsync(Guid id)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (address == null) return false;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        // Check if address is used in any orders
        var hasOrders = await _context.Orders
            .AsNoTracking()
            .AnyAsync(o => o.AddressId == id);

        if (hasOrders)
        {
            // Soft delete - just mark as inactive
            address.IsActive = false;
            address.IsDefault = false;
        }
        else
        {
            // Hard delete if no orders
            address.IsDeleted = true;
        }

        address.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetDefaultShippingAddressAsync(Guid userId, Guid addressId)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null) return false;

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // Unset other default addresses
        var existingDefault = await _context.Set<ShippingAddress>()
            .Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId)
            .ToListAsync();

        foreach (var a in existingDefault)
        {
            a.IsDefault = false;
        }

        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

