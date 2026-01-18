using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CancelOrderSplit;

public class CancelOrderSplitCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CancelOrderSplitCommandHandler> logger) : IRequestHandler<CancelOrderSplitCommand, bool>
{

    public async Task<bool> Handle(CancelOrderSplitCommand request, CancellationToken cancellationToken)
    {
        var split = await context.Set<OrderSplit>()
            .Include(s => s.SplitOrder)
            .Include(s => s.OriginalOrder)
            .FirstOrDefaultAsync(s => s.Id == request.SplitId, cancellationToken);

        if (split is null) return false;

        if (split.SplitOrder.Status != OrderStatus.Pending)
        {
            throw new BusinessException("Beklemede durumunda olmayan bölünmüş sipariş iptal edilemez.");
        }

        var splitItems = await context.Set<OrderSplitItem>()
            .AsSplitQuery()
            .Include(si => si.OriginalOrderItem)
                .ThenInclude(oi => oi.Product)
            .Include(si => si.SplitOrderItem)
                .ThenInclude(oi => oi.Product)
            .Where(si => si.OrderSplitId == request.SplitId)
            .ToListAsync(cancellationToken);

        foreach (var splitItem in splitItems)
        {
            var originalItem = splitItem.OriginalOrderItem;
            var newQuantity = originalItem.Quantity + splitItem.Quantity;
            originalItem.UpdateQuantity(newQuantity);
        }

        var originalOrder = split.OriginalOrder;
        originalOrder.RecalculateTotals();

        split.SplitOrder.MarkAsDeleted();
        split.MarkAsDeleted();

        split.Cancel();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order split cancelled. SplitId: {SplitId}", request.SplitId);
        
        return true;
    }
}
