using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.RemoveStoreRole;

public class RemoveStoreRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveStoreRoleCommandHandler> logger) : IRequestHandler<RemoveStoreRoleCommand, bool>
{
    public async Task<bool> Handle(RemoveStoreRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Removing store role. StoreRoleId: {StoreRoleId}", request.StoreRoleId);

        var storeRole = await context.Set<StoreRole>()
            .FirstOrDefaultAsync(sr => sr.Id == request.StoreRoleId && !sr.IsDeleted, ct);

        if (storeRole is null)
        {
            throw new Application.Exceptions.NotFoundException("StoreRole", request.StoreRoleId);
        }

        storeRole.Remove();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
