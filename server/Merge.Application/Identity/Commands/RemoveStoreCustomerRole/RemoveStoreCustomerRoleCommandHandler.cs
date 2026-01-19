using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.RemoveStoreCustomerRole;

public class RemoveStoreCustomerRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveStoreCustomerRoleCommandHandler> logger) : IRequestHandler<RemoveStoreCustomerRoleCommand, bool>
{
    public async Task<bool> Handle(RemoveStoreCustomerRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Removing store customer role. StoreCustomerRoleId: {StoreCustomerRoleId}", request.StoreCustomerRoleId);

        var storeCustomerRole = await context.Set<StoreCustomerRole>()
            .FirstOrDefaultAsync(scr => scr.Id == request.StoreCustomerRoleId && !scr.IsDeleted, ct);

        if (storeCustomerRole is null)
        {
            throw new Application.Exceptions.NotFoundException("StoreCustomerRole", request.StoreCustomerRoleId);
        }

        storeCustomerRole.Remove();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
