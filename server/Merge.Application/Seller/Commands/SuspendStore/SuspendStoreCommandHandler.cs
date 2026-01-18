using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.SuspendStore;

public class SuspendStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SuspendStoreCommandHandler> logger) : IRequestHandler<SuspendStoreCommand, bool>
{

    public async Task<bool> Handle(SuspendStoreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Suspending store. StoreId: {StoreId}, Reason: {Reason}",
            request.StoreId, request.Reason);

        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        store.Suspend(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store suspended. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
