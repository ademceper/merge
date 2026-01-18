using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.VerifyStore;

public class VerifyStoreCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<VerifyStoreCommandHandler> logger) : IRequestHandler<VerifyStoreCommand, bool>
{

    public async Task<bool> Handle(VerifyStoreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying store. StoreId: {StoreId}", request.StoreId);

        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        store.Verify();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Store verified. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
