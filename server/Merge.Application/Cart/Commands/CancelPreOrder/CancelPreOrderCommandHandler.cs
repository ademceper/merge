using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using PreOrder = Merge.Domain.Modules.Ordering.PreOrder;
using PreOrderCampaign = Merge.Domain.Modules.Marketing.PreOrderCampaign;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CancelPreOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelPreOrderCommandHandler> logger) : IRequestHandler<CancelPreOrderCommand, bool>
{

    public async Task<bool> Handle(CancelPreOrderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var preOrder = await context.Set<PreOrder>()
                .FirstOrDefaultAsync(po => po.Id == request.PreOrderId && po.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (preOrder is null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Siparişe dönüştürülmüş bir ön sipariş iptal edilemez.");
            }

            preOrder.Cancel();

            var campaign = await context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.ProductId == preOrder.ProductId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (campaign is not null)
            {
                campaign.DecrementQuantity(preOrder.Quantity);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "PreOrder iptal hatasi. PreOrderId: {PreOrderId}, UserId: {UserId}",
                request.PreOrderId, request.UserId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

