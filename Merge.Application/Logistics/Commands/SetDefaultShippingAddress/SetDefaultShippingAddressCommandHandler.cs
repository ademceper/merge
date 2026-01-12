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

namespace Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class SetDefaultShippingAddressCommandHandler : IRequestHandler<SetDefaultShippingAddressCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDefaultShippingAddressCommandHandler> _logger;

    public SetDefaultShippingAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetDefaultShippingAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SetDefaultShippingAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting default shipping address. UserId: {UserId}, AddressId: {AddressId}", request.UserId, request.AddressId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var address = await _context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Shipping address not found. UserId: {UserId}, AddressId: {AddressId}", request.UserId, request.AddressId);
            throw new NotFoundException("Kargo adresi", request.AddressId);
        }

        // Unset other default addresses
        var existingDefault = await _context.Set<ShippingAddress>()
            .Where(a => a.UserId == request.UserId && a.IsDefault && a.Id != request.AddressId)
            .ToListAsync(cancellationToken);

        foreach (var a in existingDefault)
        {
            a.UnsetAsDefault();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        address.SetAsDefault();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Default shipping address set successfully. UserId: {UserId}, AddressId: {AddressId}", request.UserId, request.AddressId);
        return Unit.Value;
    }
}

