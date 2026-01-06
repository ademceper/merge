using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;

namespace Merge.Application.Cart.Commands.ProcessExpiredPreOrders;

public class ProcessExpiredPreOrdersCommandHandler : IRequestHandler<ProcessExpiredPreOrdersCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessExpiredPreOrdersCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessExpiredPreOrdersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredPreOrders = await _context.Set<Domain.Entities.PreOrder>()
            .Where(po => po.Status == PreOrderStatus.Pending && po.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var preOrder in expiredPreOrders)
        {
            preOrder.MarkAsExpired();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

