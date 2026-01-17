using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.PatchAddress;

/// <summary>
/// Handler for PatchAddressCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchAddressCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PatchAddressCommandHandler> logger) : IRequestHandler<PatchAddressCommand, AddressDto>
{

    public async Task<AddressDto> Handle(PatchAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching address with ID: {AddressId}", request.Id);

        var address = await context.Set<Address>()
            .Where(a => a.Id == request.Id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            throw new NotFoundException("Address", request.Id);
        }

        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            logger.LogWarning("Unauthorized patch attempt to address {AddressId} by user {UserId}",
                request.Id, request.UserId.Value);
            throw new BusinessException("Bu adresi güncelleme yetkiniz bulunmamaktadır.");
        }

        // Apply partial updates - only update fields that are provided
        var title = request.PatchDto.Title ?? address.Title;
        var firstName = request.PatchDto.FirstName ?? address.FirstName;
        var lastName = request.PatchDto.LastName ?? address.LastName;
        var phoneNumber = request.PatchDto.PhoneNumber ?? address.PhoneNumber;
        var addressLine1 = request.PatchDto.AddressLine1 ?? address.AddressLine1;
        var addressLine2 = request.PatchDto.AddressLine2 ?? address.AddressLine2;
        var city = request.PatchDto.City ?? address.City;
        var district = request.PatchDto.District ?? address.District;
        var postalCode = request.PatchDto.PostalCode ?? address.PostalCode;
        var country = request.PatchDto.Country ?? address.Country;

        address.UpdateAddress(
            title: title,
            firstName: firstName,
            lastName: lastName,
            phoneNumber: phoneNumber,
            addressLine1: addressLine1,
            city: city,
            district: district,
            postalCode: postalCode,
            addressLine2: addressLine2);

        // IsDefault değişikliği ayrı kontrol edilmeli
        if (request.PatchDto.IsDefault.HasValue)
        {
            if (request.PatchDto.IsDefault.Value && !address.IsDefault)
            {
                // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
                var existingDefaults = await context.Set<Address>()
                    .Where(a => a.UserId == address.UserId && a.Id != request.Id && a.IsDefault && !a.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var addr in existingDefaults)
                {
                    addr.RemoveDefault();
                }

                if (existingDefaults.Count > 0)
                {
                    logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
                }

                address.SetAsDefault();
            }
            else if (!request.PatchDto.IsDefault.Value && address.IsDefault)
            {
                address.RemoveDefault();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address patched successfully with ID: {AddressId}", request.Id);

        return mapper.Map<AddressDto>(address);
    }
}
