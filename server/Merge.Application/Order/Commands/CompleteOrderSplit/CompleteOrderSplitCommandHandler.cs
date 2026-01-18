using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CompleteOrderSplit;

public class CompleteOrderSplitCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CompleteOrderSplitCommandHandler> logger) : IRequestHandler<CompleteOrderSplitCommand, bool>
{

    public async Task<bool> Handle(CompleteOrderSplitCommand request, CancellationToken cancellationToken)
    {
        var split = await context.Set<OrderSplit>()
            .FirstOrDefaultAsync(s => s.Id == request.SplitId, cancellationToken);

        if (split == null) return false;

        split.Complete();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order split completed. SplitId: {SplitId}", request.SplitId);
        
        return true;
    }
}
