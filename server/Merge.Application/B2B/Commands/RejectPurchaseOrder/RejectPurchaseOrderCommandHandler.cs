using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RejectPurchaseOrderCommandHandler> logger) : IRequestHandler<RejectPurchaseOrderCommand, bool>
{

    public async Task<bool> Handle(RejectPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Rejecting purchase order. PurchaseOrderId: {PurchaseOrderId}, Reason: {Reason}",
            request.Id, request.Reason);

        var po = await context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (po is null)
        {
            logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
            return false;
        }

        po.Reject(request.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Purchase order rejected successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
        return true;
    }
}

