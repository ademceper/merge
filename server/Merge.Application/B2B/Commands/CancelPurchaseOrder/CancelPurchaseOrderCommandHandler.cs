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

namespace Merge.Application.B2B.Commands.CancelPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CancelPurchaseOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelPurchaseOrderCommandHandler> logger) : IRequestHandler<CancelPurchaseOrderCommand, bool>
{

    public async Task<bool> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling purchase order. PurchaseOrderId: {PurchaseOrderId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (po == null)
        {
            logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (PurchaseOrder.Cancel() içinde PurchaseOrderCancelledEvent)
        po.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Purchase order cancelled successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
        return true;
    }
}

