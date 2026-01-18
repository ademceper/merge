using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.DeleteAddress;

public class DeleteAddressCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteAddressCommandHandler> logger) : IRequestHandler<DeleteAddressCommand, bool>
{

    public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Deleting address with ID: {AddressId}", request.Id);

        var address = await context.Set<Address>()
            .Where(a => a.Id == request.Id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address is null)
        {
            logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            return false;
        }
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            logger.LogWarning("Unauthorized delete attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adresi silme yetkiniz bulunmamaktadır.");
        }

                address.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        

        logger.LogInformation("Address deleted successfully with ID: {AddressId}", request.Id);

        return true;
    }
}
