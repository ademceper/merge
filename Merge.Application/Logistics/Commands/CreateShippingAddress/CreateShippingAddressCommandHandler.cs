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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateShippingAddressCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateShippingAddressCommandHandler> logger) : IRequestHandler<CreateShippingAddressCommand, ShippingAddressDto>
{

    public async Task<ShippingAddressDto> Handle(CreateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating shipping address. UserId: {UserId}, Label: {Label}", request.UserId, request.Label);

        // ✅ PERFORMANCE: AsNoTracking - Check if user exists
        // ⚠️ NOT: User entity'si BaseEntity'den türemediği için Set<UserEntity>() kullanılamaz, Users property'si kullanılmalı
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // If this is default, unset other default addresses
        if (request.IsDefault)
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            var existingDefault = await context.Set<ShippingAddress>()
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existingAddr in existingDefault)
            {
                existingAddr.UnsetAsDefault();
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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
        
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping address created successfully. AddressId: {AddressId}, UserId: {UserId}", address.Id, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ShippingAddressDto>(address);
    }
}

