using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdateShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateShippingAddressCommandHandler : IRequestHandler<UpdateShippingAddressCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateShippingAddressCommandHandler> _logger;

    public UpdateShippingAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateShippingAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating shipping address. AddressId: {AddressId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Shipping address not found. AddressId: {AddressId}", request.Id);
            throw new NotFoundException("Kargo adresi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(request.Label) ||
            !string.IsNullOrEmpty(request.FirstName) ||
            !string.IsNullOrEmpty(request.LastName) ||
            !string.IsNullOrEmpty(request.Phone) ||
            !string.IsNullOrEmpty(request.AddressLine1) ||
            !string.IsNullOrEmpty(request.City) ||
            !string.IsNullOrEmpty(request.Country))
        {
            address.UpdateDetails(
                request.Label ?? address.Label,
                request.FirstName ?? address.FirstName,
                request.LastName ?? address.LastName,
                request.Phone ?? address.Phone,
                request.AddressLine1 ?? address.AddressLine1,
                request.AddressLine2 ?? address.AddressLine2,
                request.City ?? address.City,
                request.State ?? address.State ?? string.Empty,
                request.PostalCode ?? address.PostalCode ?? string.Empty,
                request.Country ?? address.Country ?? string.Empty,
                request.Instructions ?? address.Instructions);
        }

        if (request.IsDefault.HasValue && request.IsDefault.Value)
        {
            // Unset other default addresses
            var existingDefault = await _context.Set<ShippingAddress>()
                .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != request.Id)
                .ToListAsync(cancellationToken);

            foreach (var a in existingDefault)
            {
                a.UnsetAsDefault();
            }

            address.SetAsDefault();
        }
        else if (request.IsDefault.HasValue && !request.IsDefault.Value)
        {
            address.UnsetAsDefault();
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                address.Activate();
            }
            else
            {
                address.Deactivate();
            }
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shipping address updated successfully. AddressId: {AddressId}", request.Id);
        return Unit.Value;
    }
}

