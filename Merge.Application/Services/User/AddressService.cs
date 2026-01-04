using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.User;

public class AddressService : IAddressService
{
    private readonly IRepository<Address> _addressRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddressService> _logger;

    public AddressService(
        IRepository<Address> addressRepository,
        ApplicationDbContext context,
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

        var address = await _context.Addresses
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

        var addresses = await _context.Addresses
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
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == dto.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }

            if (existingDefaults.Any())
            {
                _logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        var address = _mapper.Map<Address>(dto);
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
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == address.UserId && a.Id != id && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }

            if (existingDefaults.Any())
            {
                _logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        address.Title = dto.Title;
        address.FirstName = dto.FirstName;
        address.LastName = dto.LastName;
        address.PhoneNumber = dto.PhoneNumber;
        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.District = dto.District;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;
        address.IsDefault = dto.IsDefault;

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

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", id, userId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var existingDefaults = await _context.Addresses
            .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var addr in existingDefaults)
        {
            addr.IsDefault = false;
        }

        address.IsDefault = true;

        await _addressRepository.UpdateAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", id, existingDefaults.Count);

        return true;
    }
}

