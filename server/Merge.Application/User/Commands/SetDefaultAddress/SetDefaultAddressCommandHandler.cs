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

public class SetDefaultAddressCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SetDefaultAddressCommandHandler> logger) : IRequestHandler<SetDefaultAddressCommand, bool>
{

    public async Task<bool> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Setting address {AddressId} as default for user {UserId}", request.Id, request.UserId);

        var address = await context.Set<Address>()
            .Where(a => a.Id == request.Id && a.UserId == request.UserId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId} for user: {UserId}", request.Id, request.UserId);
            return false;
        }

        // Diğer adreslerin default'unu kaldır
        var existingDefaults = await context.Set<Address>()
            .Where(a => a.UserId == request.UserId && a.Id != request.Id && a.IsDefault && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var addr in existingDefaults)
        {
            addr.RemoveDefault();
        }

                address.SetAsDefault();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        

        logger.LogInformation("Address {AddressId} set as default successfully. Cleared {Count} previous defaults", 
            request.Id, existingDefaults.Count);

        return true;
    }
}
