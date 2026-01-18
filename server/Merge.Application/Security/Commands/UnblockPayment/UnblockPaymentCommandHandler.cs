using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.UnblockPayment;

public class UnblockPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UnblockPaymentCommandHandler> logger) : IRequestHandler<UnblockPaymentCommand, bool>
{

    public async Task<bool> Handle(UnblockPaymentCommand request, CancellationToken cancellationToken)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);

        if (check == null) return false;

        check.Unblock();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payment unblocked. CheckId: {CheckId}", request.CheckId);

        return true;
    }
}
