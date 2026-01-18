using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.CreateShippingAddress;

public class CreateShippingAddressCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateShippingAddressCommandHandler> logger) : IRequestHandler<CreateShippingAddressCommand, ShippingAddressDto>
{

    public async Task<ShippingAddressDto> Handle(CreateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating shipping address. UserId: {UserId}, Label: {Label}", request.UserId, request.Label);

        // ⚠️ NOT: User entity'si BaseEntity'den türemediği için Set<UserEntity>() kullanılamaz, Users property'si kullanılmalı
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // If this is default, unset other default addresses
        if (request.IsDefault)
        {
            var existingDefault = await context.Set<ShippingAddress>()
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existingAddr in existingDefault)
            {
                existingAddr.UnsetAsDefault();
            }
        }

        var address = ShippingAddress.Create(
            request.UserId,
            request.Label,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country ?? string.Empty,
            request.IsDefault,
            request.Instructions);

        await context.Set<ShippingAddress>().AddAsync(address, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping address created successfully. AddressId: {AddressId}, UserId: {UserId}", address.Id, request.UserId);

        return mapper.Map<ShippingAddressDto>(address);
    }
}

