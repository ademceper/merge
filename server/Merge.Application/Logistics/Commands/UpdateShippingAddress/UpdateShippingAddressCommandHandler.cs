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

public class UpdateShippingAddressCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateShippingAddressCommandHandler> logger) : IRequestHandler<UpdateShippingAddressCommand, Unit>
{

    public async Task<Unit> Handle(UpdateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating shipping address. AddressId: {AddressId}", request.Id);

        var address = await context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Shipping address not found. AddressId: {AddressId}", request.Id);
            throw new NotFoundException("Kargo adresi", request.Id);
        }

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

        if (request.IsDefault.HasValue)
        {
            if (request.IsDefault.Value)
            {
                await SetDefaultAddressAsync(address, request.Id, cancellationToken);
            }
            else
            {
                address.UnsetAsDefault();
            }
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping address updated successfully. AddressId: {AddressId}", request.Id);
        return Unit.Value;
    }

    private async Task SetDefaultAddressAsync(ShippingAddress address, Guid requestId, CancellationToken cancellationToken)
    {
        // Unset other default addresses
        var existingDefault = await context.Set<ShippingAddress>()
            .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != requestId)
            .ToListAsync(cancellationToken);

        foreach (var a in existingDefault)
        {
            a.UnsetAsDefault();
        }

        address.SetAsDefault();
    }
}

