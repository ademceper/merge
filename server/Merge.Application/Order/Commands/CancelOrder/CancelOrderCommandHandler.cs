using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CancelOrder;

public class CancelOrderCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CancelOrderCommandHandler> logger) : IRequestHandler<CancelOrderCommand, bool>
{

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
        {
            throw new BusinessException("Bu sipariş iptal edilemez.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in order.OrderItems)
            {
                item.Product.IncreaseStock(item.Quantity);
            }

            order.Cancel();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Order cancelled successfully. OrderId: {OrderId}, ItemsRestored: {ItemCount}",
                request.OrderId, order.OrderItems.Count);

            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Order cancellation failed. OrderId: {OrderId}", request.OrderId);
            throw;
        }
    }
}
