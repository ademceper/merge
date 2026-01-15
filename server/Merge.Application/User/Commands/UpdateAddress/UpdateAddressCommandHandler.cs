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

namespace Merge.Application.User.Commands.UpdateAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAddressCommandHandler> _logger;

    public UpdateAddressCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation("Updating address with ID: {AddressId}", request.Id);

        var address = await _context.Set<Merge.Domain.Modules.Identity.Address>()
            .Where(a => a.Id == request.Id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            throw new Application.Exceptions.NotFoundException("Address", request.Id);
        }
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            _logger.LogWarning("Unauthorized update attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adresi güncelleme yetkiniz bulunmamaktadır.");
        }

        // Eğer default olarak işaretleniyorsa, diğer adreslerin default'unu kaldır
        if (request.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await _context.Set<Merge.Domain.Modules.Identity.Address>()
                .Where(a => a.UserId == address.UserId && a.Id != request.Id && a.IsDefault && !a.IsDeleted)
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event\'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır

        _logger.LogInformation("Address updated successfully with ID: {AddressId}", request.Id);

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AddressDto>(address);
    }
}
