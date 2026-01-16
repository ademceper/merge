using AutoMapper;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Address = Merge.Domain.Modules.Identity.Address;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using IRepository = Merge.Application.Interfaces.IRepository<AddressEntity>;


namespace Merge.Application.Services.User;

public class AddressService : IAddressService
{
    private readonly IRepository _addressRepository;
    private readonly IDbContext _context; // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddressService> _logger;

    public AddressService(
        IRepository addressRepository,
        IDbContext context, // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AddressDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving address with ID: {AddressId}", id);

        var address = await _context.Set<AddressEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId}", id);
            return null;
        }

        return _mapper.Map<AddressDto>(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving addresses for user ID: {UserId}", userId);

        var addresses = await _context.Set<AddressEntity>()
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} addresses for user ID: {UserId}", addresses.Count, userId);

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AddressDto> CreateAsync(CreateAddressDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.AddressLine1))
        {
            throw new ValidationException("Adres satırı boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(dto.City))
        {
            throw new ValidationException("Şehir boş olamaz.");
        }

        _logger.LogInformation("Creating new address for user ID: {UserId}", dto.UserId);

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (dto.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var existingDefaults = await _context.Set<AddressEntity>()
                .Where(a => a.UserId == dto.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
            }

            if (existingDefaults.Any())
            {
                _logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        // ✅ BOLUM 11.0: Rich Domain Model - Factory method kullan
        var address = Address.Create(
            userId: dto.UserId,
            title: dto.Title ?? string.Empty,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            phoneNumber: dto.PhoneNumber,
            addressLine1: dto.AddressLine1,
            city: dto.City,
            district: dto.District,
            postalCode: dto.PostalCode,
            country: dto.Country ?? "Türkiye",
            addressLine2: dto.AddressLine2,
            isDefault: dto.IsDefault);
        
        address = await _addressRepository.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address created successfully with ID: {AddressId}", address.Id);

        return _mapper.Map<AddressDto>(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AddressDto> UpdateAsync(Guid id, UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.AddressLine1))
        {
            throw new ValidationException("Adres satırı boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(dto.City))
        {
            throw new ValidationException("Şehir boş olamaz.");
        }

        _logger.LogInformation("Updating address with ID: {AddressId}", id);

        var address = await _addressRepository.GetByIdAsync(id, cancellationToken);
        if (address == null)
        {
            _logger.LogWarning("Address not found for update with ID: {AddressId}", id);
            throw new NotFoundException("Adres", id);
        }

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (dto.IsDefault && !address.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var existingDefaults = await _context.Set<AddressEntity>()
                .Where(a => a.UserId == address.UserId && a.Id != id && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
            }

            if (existingDefaults.Any())
            {
                _logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        address.UpdateAddress(
            title: dto.Title ?? string.Empty,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            phoneNumber: dto.PhoneNumber,
            addressLine1: dto.AddressLine1,
            city: dto.City,
            district: dto.District,
            postalCode: dto.PostalCode,
            addressLine2: dto.AddressLine2);
        
        // IsDefault değişikliği ayrı kontrol edilmeli
        if (dto.IsDefault && !address.IsDefault)
        {
            address.SetAsDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        }
        else if (!dto.IsDefault && address.IsDefault)
        {
            address.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        }

        await _addressRepository.UpdateAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address updated successfully with ID: {AddressId}", id);

        return _mapper.Map<AddressDto>(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting address with ID: {AddressId}", id);

        var address = await _addressRepository.GetByIdAsync(id, cancellationToken);
        if (address == null)
        {
            _logger.LogWarning("Address not found for deletion with ID: {AddressId}", id);
            return false;
        }

        await _addressRepository.DeleteAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address deleted successfully with ID: {AddressId}", id);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SetDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting address {AddressId} as default for user {UserId}", id, userId);

        var address = await _context.Set<AddressEntity>()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", id, userId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var existingDefaults = await _context.Set<AddressEntity>()
            .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var addr in existingDefaults)
        {
            addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        }

        address.SetAsDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan

        await _addressRepository.UpdateAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", id, existingDefaults.Count);

        return true;
    }
}

