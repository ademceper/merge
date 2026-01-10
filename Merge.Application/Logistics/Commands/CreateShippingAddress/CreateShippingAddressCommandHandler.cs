using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Entities.User;

namespace Merge.Application.Logistics.Commands.CreateShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateShippingAddressCommandHandler : IRequestHandler<CreateShippingAddressCommand, ShippingAddressDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateShippingAddressCommandHandler> _logger;

    public CreateShippingAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateShippingAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingAddressDto> Handle(CreateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating shipping address. UserId: {UserId}, Label: {Label}", request.UserId, request.Label);

        // ✅ PERFORMANCE: AsNoTracking - Check if user exists
        // ⚠️ NOT: User entity'si BaseEntity'den türemediği için Set<UserEntity>() kullanılamaz, Users property'si kullanılmalı
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // If this is default, unset other default addresses
        if (request.IsDefault)
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            var existingDefault = await _context.Set<ShippingAddress>()
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
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.AddressLine2,
            request.IsDefault,
            request.Instructions);

        await _context.Set<ShippingAddress>().AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shipping address created successfully. AddressId: {AddressId}, UserId: {UserId}", address.Id, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingAddressDto>(address);
    }
}

