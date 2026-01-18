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
using Address = Merge.Domain.Modules.Identity.Address;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.CreateAddress;

public class CreateAddressCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateAddressCommandHandler> logger) : IRequestHandler<CreateAddressCommand, AddressDto>
{

    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Creating new address for user ID: {UserId}", request.UserId);

        // Eger default olarak isaretleniyorsa, diger adreslerin default'unu kaldir
        if (request.IsDefault)
        {
            var existingDefaults = await context.Set<AddressEntity>()
                .Where(a => a.UserId == request.UserId && a.IsDefault && !a.IsDeleted)
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

        var address = AddressEntity.Create(
            userId: request.UserId,
            title: request.Title,
            firstName: request.FirstName,
            lastName: request.LastName,
            phoneNumber: request.PhoneNumber,
            addressLine1: request.AddressLine1,
            city: request.City,
            district: request.District,
            postalCode: request.PostalCode,
            country: request.Country,
            addressLine2: request.AddressLine2,
            isDefault: request.IsDefault);

        await context.Set<AddressEntity>().AddAsync(address, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address created successfully with ID: {AddressId}", address.Id);

        return mapper.Map<AddressDto>(address);
    }
}
