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

    public async Task<AddressDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving address with ID: {AddressId}", id);

        var address = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId}", id);
            return null;
        }

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving addresses for user ID: {UserId}", userId);

        var addresses = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Found {Count} addresses for user ID: {UserId}", addresses.Count, userId);

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto> CreateAsync(CreateAddressDto dto)
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
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == dto.UserId && a.IsDefault)
                .ToListAsync();

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
        address = await _addressRepository.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address created successfully with ID: {AddressId}", address.Id);

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> UpdateAsync(Guid id, UpdateAddressDto dto)
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

        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
        {
            _logger.LogWarning("Address not found for update with ID: {AddressId}", id);
            throw new NotFoundException("Adres", id);
        }

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (dto.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == address.UserId && a.Id != id && a.IsDefault)
                .ToListAsync();

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

        await _addressRepository.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address updated successfully with ID: {AddressId}", id);

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting address with ID: {AddressId}", id);

        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
        {
            _logger.LogWarning("Address not found for deletion with ID: {AddressId}", id);
            return false;
        }

        await _addressRepository.DeleteAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address deleted successfully with ID: {AddressId}", id);

        return true;
    }

    public async Task<bool> SetDefaultAsync(Guid id, Guid userId)
    {
        _logger.LogInformation("Setting address {AddressId} as default for user {UserId}", id, userId);

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", id, userId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        var existingDefaults = await _context.Addresses
            .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
            .ToListAsync();

        foreach (var addr in existingDefaults)
        {
            addr.IsDefault = false;
        }

        address.IsDefault = true;

        await _addressRepository.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", id, existingDefaults.Count);

        return true;
    }
}

