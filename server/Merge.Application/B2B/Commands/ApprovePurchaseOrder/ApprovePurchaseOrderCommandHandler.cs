using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ApprovePurchaseOrderCommandHandler> logger) : IRequestHandler<ApprovePurchaseOrderCommand, bool>
{

    public async Task<bool> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving purchase order. PurchaseOrderId: {PurchaseOrderId}, ApprovedByUserId: {ApprovedByUserId}",
            request.Id, request.ApprovedByUserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var po = await context.Set<PurchaseOrder>()
                .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

            if (po is null)
            {
                logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // Check credit limit if credit term is used
            if (po.CreditTermId.HasValue)
            {
                var creditTerm = await context.Set<CreditTerm>()
                    .FirstOrDefaultAsync(ct => ct.Id == po.CreditTermId.Value, cancellationToken);

                if (creditTerm is not null && creditTerm.CreditLimit.HasValue)
                {
                    // Entity method içinde zaten credit limit kontrolü var
                    creditTerm.UseCredit(po.TotalAmount);
                }
            }

            po.Approve(request.ApprovedByUserId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Purchase order approved successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "PurchaseOrder onaylama hatasi. PurchaseOrderId: {PurchaseOrderId}, ApprovedByUserId: {ApprovedByUserId}",
                request.Id, request.ApprovedByUserId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

