using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Logistics;

public class ShippingAddressService : IShippingAddressService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ShippingAddressService> _logger;

    public ShippingAddressService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShippingAddressService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ShippingAddressDto> CreateShippingAddressAsync(Guid userId, CreateShippingAddressDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Kargo adresi olusturuluyor. UserId: {UserId}", userId);

        try
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

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
                    .ToListAsync(cancellationToken);

                foreach (var existingAddr in existingDefault)
                {
                    existingAddr.UnsetAsDefault();
                }
            }

            // Factory method kullan
            var address = ShippingAddress.Create(
                userId,
                dto.Label,
                dto.FirstName,
                dto.LastName,
                dto.Phone,
                dto.AddressLine1,
                dto.AddressLine2,
                dto.City,
                dto.State ?? string.Empty,
                dto.PostalCode ?? string.Empty,
                dto.Country ?? string.Empty,
                dto.IsDefault,
                dto.Instructions);

            await _context.Set<ShippingAddress>().AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Kargo adresi olusturuldu. AddressId: {AddressId}, UserId: {UserId}", address.Id, userId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<ShippingAddressDto>(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kargo adresi olusturma hatasi. UserId: {UserId}", userId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ShippingAddressDto?> GetShippingAddressByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ShippingAddressDto>> GetUserShippingAddressesAsync(Guid userId, bool? isActive = null, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ShippingAddressDto>>(addresses);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ShippingAddressDto?> GetDefaultShippingAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateShippingAddressAsync(Guid id, UpdateShippingAddressDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address == null) return false;

        // Domain method kullan - mevcut değerleri kullan, sadece değişenleri güncelle
        address.UpdateDetails(
            !string.IsNullOrEmpty(dto.Label) ? dto.Label : address.Label,
            !string.IsNullOrEmpty(dto.FirstName) ? dto.FirstName : address.FirstName,
            !string.IsNullOrEmpty(dto.LastName) ? dto.LastName : address.LastName,
            dto.Phone ?? address.Phone,
            !string.IsNullOrEmpty(dto.AddressLine1) ? dto.AddressLine1 : address.AddressLine1,
            dto.AddressLine2 ?? address.AddressLine2,
            !string.IsNullOrEmpty(dto.City) ? dto.City : address.City,
            dto.State ?? address.State,
            dto.PostalCode ?? address.PostalCode,
            !string.IsNullOrEmpty(dto.Country) ? dto.Country : address.Country,
            dto.Instructions ?? address.Instructions);

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            // Unset other default addresses
            var existingDefault = await _context.Set<ShippingAddress>()
                .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != id)
                .ToListAsync(cancellationToken);

            foreach (var a in existingDefault)
            {
                a.UnsetAsDefault();
            }

            address.SetAsDefault();
        }
        else if (dto.IsDefault.HasValue && !dto.IsDefault.Value)
        {
            address.UnsetAsDefault();
        }

        if (dto.IsActive.HasValue && dto.IsActive.Value && !address.IsActive)
        {
            address.Activate();
        }
        else if (dto.IsActive.HasValue && !dto.IsActive.Value && address.IsActive)
        {
            address.Deactivate();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteShippingAddressAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address == null) return false;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        // Check if address is used in any orders
        var hasOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .AnyAsync(o => o.AddressId == id, cancellationToken);

        if (hasOrders)
        {
            // Soft delete - just mark as inactive
            address.Deactivate();
            address.UnsetAsDefault();
        }
        else
        {
            // Hard delete if no orders - Domain method kullan
            address.MarkAsDeleted();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SetDefaultShippingAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address == null) return false;

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // Unset other default addresses
        var existingDefault = await _context.Set<ShippingAddress>()
            .Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId)
            .ToListAsync(cancellationToken);

        foreach (var a in existingDefault)
        {
            a.UnsetAsDefault();
        }

        // Domain method kullan
        address.SetAsDefault();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

