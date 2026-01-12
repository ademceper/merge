using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.SetDefaultAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDefaultAddressCommandHandler> _logger;

    public SetDefaultAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetDefaultAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting address {AddressId} as default for user {UserId}", request.Id, request.UserId);

        var address = await _context.Set<Address>()
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == request.UserId, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", request.Id, request.UserId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        var existingDefaults = await _context.Set<Address>()
            .Where(a => a.UserId == request.UserId && a.Id != request.Id && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var addr in existingDefaults)
        {
            addr.RemoveDefault();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        address.SetAsDefault();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", 
            request.Id, existingDefaults.Count);

        return true;
    }
}
