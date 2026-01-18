using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.UpdateAddress;

public class UpdateAddressCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateAddressCommandHandler> logger) : IRequestHandler<UpdateAddressCommand, AddressDto>
{

    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Updating address with ID: {AddressId}", request.Id);

        var address = await context.Set<AddressEntity>()
            .Where(a => a.Id == request.Id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            throw new Application.Exceptions.NotFoundException("Address", request.Id);
        }
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            logger.LogWarning("Unauthorized update attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adresi güncelleme yetkiniz bulunmamaktadır.");
        }

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (request.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await context.Set<AddressEntity>()
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
        }

                address.UpdateAddress(
            title: request.Title,
            firstName: request.FirstName,
            lastName: request.LastName,
            phoneNumber: request.PhoneNumber,
            addressLine1: request.AddressLine1,
            city: request.City,
            district: request.District,
            postalCode: request.PostalCode,
            addressLine2: request.AddressLine2);

        // IsDefault değişikliği ayrı kontrol edilmeli
        if (request.IsDefault && !address.IsDefault)
        {
            address.SetAsDefault();
        }
        else if (!request.IsDefault && address.IsDefault)
        {
            address.RemoveDefault();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        

        logger.LogInformation("Address updated successfully with ID: {AddressId}", request.Id);

        return mapper.Map<AddressDto>(address);
    }
}
