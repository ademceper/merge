using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using PreOrder = Merge.Domain.Modules.Ordering.PreOrder;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.ProcessExpiredPreOrders;

public class ProcessExpiredPreOrdersCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcessExpiredPreOrdersCommand>
{

    public async Task Handle(ProcessExpiredPreOrdersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredPreOrders = await context.Set<PreOrder>()
            .Where(po => po.Status == PreOrderStatus.Pending && po.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var preOrder in expiredPreOrders)
        {
            preOrder.MarkAsExpired();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

