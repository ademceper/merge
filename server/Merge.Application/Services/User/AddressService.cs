using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Identity.Address>;


namespace Merge.Application.Services.User;

public class AddressService(
    IRepository addressRepository,
    IDbContext context, // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<AddressService> logger) : IAddressService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AddressDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving address with ID: {AddressId}", id);

        var address = await context.Set<AddressEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId}", id);
            return null;
        }

        return mapper.Map<AddressDto>(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving addresses for user ID: {UserId}", userId);

        var addresses = await context.Set<AddressEntity>()
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} addresses for user ID: {UserId}", addresses.Count, userId);

        return mapper.Map<IEnumerable<AddressDto>>(addresses);
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

        logger.LogInformation("Creating new address for user ID: {UserId}", dto.UserId);

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (dto.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var existingDefaults = await context.Set<AddressEntity>()
                .Where(a => a.UserId == dto.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
            }

            if (existingDefaults.Any())
            {
                logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        // ✅ BOLUM 11.0: Rich Domain Model - Factory method kullan
        var address = AddressEntity.Create(
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
        
        address = await addressRepository.AddAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address created successfully with ID: {AddressId}", address.Id);

        return mapper.Map<AddressDto>(address);
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

        logger.LogInformation("Updating address with ID: {AddressId}", id);

        var address = await addressRepository.GetByIdAsync(id, cancellationToken);
        if (address == null)
        {
            logger.LogWarning("Address not found for update with ID: {AddressId}", id);
            throw new NotFoundException("Adres", id);
        }

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (dto.IsDefault && !address.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var existingDefaults = await context.Set<AddressEntity>()
                .Where(a => a.UserId == address.UserId && a.Id != id && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
            }

            if (existingDefaults.Any())
            {
                logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
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

        await addressRepository.UpdateAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address updated successfully with ID: {AddressId}", id);

        return mapper.Map<AddressDto>(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting address with ID: {AddressId}", id);

        var address = await addressRepository.GetByIdAsync(id, cancellationToken);
        if (address == null)
        {
            logger.LogWarning("Address not found for deletion with ID: {AddressId}", id);
            return false;
        }

        await addressRepository.DeleteAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address deleted successfully with ID: {AddressId}", id);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SetDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Setting address {AddressId} as default for user {UserId}", id, userId);

        var address = await context.Set<AddressEntity>()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", id, userId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var existingDefaults = await context.Set<AddressEntity>()
            .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var addr in existingDefaults)
        {
            addr.RemoveDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan
        }

        address.SetAsDefault(); // ✅ BOLUM 11.0: Rich Domain Model - Domain method kullan

        await addressRepository.UpdateAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", id, existingDefaults.Count);

        return true;
    }
}

