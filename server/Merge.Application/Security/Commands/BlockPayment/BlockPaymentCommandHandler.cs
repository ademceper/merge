using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.BlockPayment;

public class BlockPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<BlockPaymentCommandHandler> logger) : IRequestHandler<BlockPaymentCommand, bool>
{

    public async Task<bool> Handle(BlockPaymentCommand request, CancellationToken cancellationToken)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);

        if (check is null) return false;

        check.Block(request.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payment blocked. CheckId: {CheckId}, Reason: {Reason}", request.CheckId, request.Reason);

        return true;
    }
}
