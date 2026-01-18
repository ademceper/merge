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

namespace Merge.Application.B2B.Commands.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<SubmitPurchaseOrderCommandHandler> logger) : IRequestHandler<SubmitPurchaseOrderCommand, bool>
{

    public async Task<bool> Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Submitting purchase order. PurchaseOrderId: {PurchaseOrderId}", request.Id);

        var po = await context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (po == null)
        {
            logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
            return false;
        }

        po.Submit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Purchase order submitted successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
        return true;
    }
}

