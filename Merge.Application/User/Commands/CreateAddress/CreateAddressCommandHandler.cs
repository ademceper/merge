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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.CreateAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAddressCommandHandler> _logger;

    public CreateAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new address for user ID: {UserId}", request.UserId);

        // Eger default olarak isaretleniyorsa, diger adreslerin default'unu kaldir
        if (request.IsDefault)
        {
            var existingDefaults = await _context.Set<Merge.Domain.Modules.Identity.Address>()
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.RemoveDefault();
            }

            if (existingDefaults.Count > 0)
            {
                _logger.LogInformation("Removed default flag from {Count} existing addresses", existingDefaults.Count);
            }
        }

        var address = Merge.Domain.Modules.Identity.Address.Create(
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

        await _context.Set<Merge.Domain.Modules.Identity.Address>().AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address created successfully with ID: {AddressId}", address.Id);

        return _mapper.Map<AddressDto>(address);
    }
}
